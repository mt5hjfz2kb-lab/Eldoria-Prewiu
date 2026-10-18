// Independent World4X spatial identity contract. Does not import Unity or touch a save store.
// Same world cell + stable ID is shared by global, regional and local zoom.
export const WORLD_SIZE = 128;
export const CHUNK_SIZE = 16;
export const ZOOM_LEVELS = Object.freeze({ global: 80, regional: 28, local: 10 });

export function assertWorldPoint(point) {
  if (!point || typeof point.id !== "string" || !/^[a-z0-9][a-z0-9:-]*$/.test(point.id)) throw new Error("Invalid stable entity ID");
  if (!Number.isInteger(point.x) || !Number.isInteger(point.z) || point.x < 0 || point.z < 0 || point.x >= WORLD_SIZE || point.z >= WORLD_SIZE)
    throw new Error("World cell outside 128x128");
  return Object.freeze({ id: point.id, x: point.x, z: point.z });
}

export function chunkOf(point) {
  const p=assertWorldPoint(point);
  return Object.freeze({ x: Math.floor(p.x / CHUNK_SIZE), z: Math.floor(p.z / CHUNK_SIZE) });
}

export function worldCatalog(points) {
  const byId=new Map(), byCell=new Set();
  for (const p of points) {
    const entity=assertWorldPoint(p), cell=`${entity.x}:${entity.z}`;
    if (byId.has(entity.id) || byCell.has(cell)) throw new Error("Duplicate world identity or occupied cell");
    byId.set(entity.id,entity); byCell.add(cell);
  }
  return Object.freeze({
    get: id=>byId.get(id)??null,
    atZoom: (zoom,id)=>{
      if (!Object.hasOwn(ZOOM_LEVELS, zoom)) throw new Error("Unsupported zoom");
      return byId.get(id)??null; // Camera changes; world position deliberately does not.
    },
    points: ()=>Object.freeze([...byId.values()]),
  });
}

export function validateWorldLayout(points) {
  const catalog=worldCatalog(points);
  for (const point of catalog.points()) {
    for (const zoom of Object.keys(ZOOM_LEVELS)) {
      const current=catalog.atZoom(zoom,point.id);
      if(current!==point) throw new Error("Zoom changed world entity identity");
    }
  }
  return Object.freeze({ entities: catalog.points().length, zooms: Object.keys(ZOOM_LEVELS).length, hasStableWorldCoordinates: true });
}
