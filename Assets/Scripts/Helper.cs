using UnityEngine;

namespace Assets.Scripts
{
    public static class Helper
    {
        public static GameObject Find(this GameObject go, string nameToFind, bool bSearchInChildren)
        {
            if (bSearchInChildren)
            {
                var transform = go.transform;
                var childCount = transform.childCount;
                for (var i = 0; i < childCount; ++i)
                {
                    var child = transform.GetChild(i);
                    if (child.gameObject.name == nameToFind)
                        return child.gameObject;
                    var result = child.gameObject.Find(nameToFind, true);
                    if (result != null) return result;
                }
                return null;
            }

            return GameObject.Find(nameToFind);
        }
    }
}
