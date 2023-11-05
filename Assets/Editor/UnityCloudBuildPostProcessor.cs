using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEditor.Callbacks;

public class UnityCloudBuildPostProcessor
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string buildPath)
    {
        // ���������, ��� ������ ������������� ��� iOS
        if (buildTarget == BuildTarget.iOS)
        {
            // ���� � ����� Info.plist
            string plistPath = buildPath + "/Info.plist";

            // ������� ������ ��� ������ � ������ Info.plist
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            // ��������� ���������� ��� �������� ������
            plist.root.SetString("NSAppTransportSecurity", "<dict><key>NSAllowsArbitraryLoads</key><true/></dict>");

            // ���������� ��������� ������� � ���� Info.plist
            plist.WriteToFile(plistPath);
        }
    }
}
