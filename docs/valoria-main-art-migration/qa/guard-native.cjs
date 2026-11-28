const fs = require('node:fs');

module.exports = async function verify({ github, context, core }, requestPath) {
  const id = 'valoria-main-art-substitution-20261128';
  const owner = 'codex-valoria-main-art-20261128';
  const branch = 'integration/valoria-main-art-20261128';
  const resource = 'windows-runner-heavy';
  const source = {
    main: 'd74f1a29d5472e9037d8534142768d4c13667ddd',
    donor: '6057c2604de84ae27f47ede44eafa294c96bcd24',
    integration: context.sha
  };
  const request = JSON.parse(fs.readFileSync(requestPath, 'utf8'));
  if (context.eventName !== 'push' || context.ref !== 'refs/heads/' + branch ||
      !Object.hasOwn(source, request.target) ||
      !Number.isSafeInteger(request.ownership_revision) || request.ownership_revision < 1 ||
      typeof request.request_id !== 'string' || !/^[a-z0-9-]{1,64}$/.test(request.request_id)) {
    throw new Error('Malformed or inapplicable native capture request');
  }
  const [{ data: tip }, { data: latest }] = await Promise.all(
    [branch, 'main'].map(name => github.rest.git.getRef({ ...context.repo, ref: 'heads/' + name }))
  );
  if (tip.object.sha !== context.sha) throw new Error('Integration HEAD advanced; new request required');
  const { data: entry } = await github.rest.repos.getContent({
    ...context.repo, ref: latest.object.sha, path: 'pipeline/active-workstreams.json'
  });
  const { data: blob } = await github.rest.git.getBlob({ ...context.repo, file_sha: entry.sha });
  if (blob.sha !== entry.sha || blob.encoding !== 'base64') throw new Error('Unverifiable Main registry');
  const reg = JSON.parse(Buffer.from(blob.content, 'base64').toString('utf8'));
  const held = reg.active.filter(item => (item.resources || []).includes(resource));
  const claim = held[0];
  const rotation = reg.resource_rotation;
  if (held.length !== 1 || claim.id !== id || claim.owner !== owner ||
      claim.ownership_state !== 'claimed' || claim.branch !== branch ||
      claim.ownership_revision !== request.ownership_revision ||
      rotation?.resource !== resource || rotation.current_workstream !== id ||
      (rotation.current_run_id != null && String(rotation.current_run_id) !== String(context.runId))) {
    throw new Error('Windows lease, owner, revision or resource rotation no longer matches this run');
  }
  core.setOutput('target', request.target);
  core.setOutput('source', source[request.target]);
  core.info('Native request ' + request.request_id + ', source ' + source[request.target] +
    ', registry commit ' + latest.object.sha + ', revision ' + request.ownership_revision);
};
