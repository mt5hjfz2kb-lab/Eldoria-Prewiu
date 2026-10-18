using Eldoria.Domain;
using UnityEngine;
namespace Eldoria.Presentation {
 // Opt-in host: existing HUD/rules can present an authored scene without regenerating it.
 public interface IPlayableSceneHost {
  Camera PlayerCamera { get; }
  void Apply(PlayerState state);
  void Home(); void Pan(Vector2 delta); void Zoom(float delta); void Focus(GameObject target);
  void Navigate(string target); string TargetName(string id); WorldHotspot Pick(Vector2 point);
 }
}
