using UnityEditor.iOS.Xcode;
namespace MiniGameSDK
{
    public static class PBXHelper
    {
        public static string GetPBXTargetGuid(this PBXProject project)
        {
#if UNITY_2019_3_OR_NEWER
            string targetGUID = project.GetUnityFrameworkTargetGuid();
#else
            string targetName = PBXProject.GetUnityTargetName();
            string targetGUID = project.TargetGuidByName(targetName);
#endif
            return targetGUID;
        }
    }
}
