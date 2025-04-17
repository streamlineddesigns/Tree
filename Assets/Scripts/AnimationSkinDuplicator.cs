using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimationSkinDuplicator : MonoBehaviour
{
    [MenuItem("Tools/StudioByStorm/CloneSpriteSheetAnimation")]
    public static void SwapAnimationSkin()
    {
        string originalAnimPath = "Assets/Animation/Character Animations/Blink/BlinkAnimation.anim";  // Replace with your original animation
        string oldSkinPrefix = "V46";
        string[] newSkinPrefix = new string[50]{"V1","V2","V3","V4","V5","V6","V7","V8","V9","V10","V11","V12","V13","V14","V15","V16","V17","V18","V19","V20","V21","V22","V23","V24","V25",
                                                "V26","V27","V28","V29","V30","V31","V32","V33","V34","V35","V36","V37","V38","V39","V40","V41","V42","V43","V44","V45","V46","V47","V48","V49","V50"};

        AnimationClip originalClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(originalAnimPath);
        

        string[] spriteSheetPaths = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/SpriteSheets" });

        Dictionary<string, Sprite> spriteLookup = new Dictionary<string, Sprite>();

        foreach (string guid in spriteSheetPaths)
        {
            string texturePath = AssetDatabase.GUIDToAssetPath(guid);

            // Load all sprites associated with this sprite sheet
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);

            foreach (Object asset in assets)
            {
                if (asset is Sprite sprite)
                {
                    spriteLookup[sprite.name] = sprite;
                    //Debug.Log(sprite.name);
                }
            }
        }


        for (int a = 0; a < newSkinPrefix.Length; a++) {
            AnimationClip newClip = new AnimationClip();
            EditorUtility.CopySerialized(originalClip, newClip);

            // Duplicate name & path
            string newClipPath = "Assets/Animation/Character Animations/Blink/" + newSkinPrefix[a] + ".anim";
            AssetDatabase.CreateAsset(newClip, newClipPath);

            // Swap the keyframes
            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(newClip);
            foreach (var binding in bindings)
            {
                ObjectReferenceKeyframe[] keyframes = AnimationUtility.GetObjectReferenceCurve(newClip, binding);

                for (int i = 0; i < keyframes.Length; i++) 
                {
                    Sprite oldSprite = keyframes[i].value as Sprite;
                    if (oldSprite == null) continue;

                    string newName = oldSprite.name.Replace(oldSkinPrefix, newSkinPrefix[a]);
                    //Debug.Log(newName);
                    if (spriteLookup.ContainsKey(newName))
                    {
                        keyframes[i].value = spriteLookup[newName];
                    }
                }


                // Save updated keyframes back into the new animation
                AnimationUtility.SetObjectReferenceCurve(newClip, binding, keyframes);
            }
        }
        

        AssetDatabase.SaveAssets();
        Debug.Log("Swapped animation skin!");
    }
}
