

using Microsoft.Win32;

namespace poligon_pezeglądarka_grafiki.Model;

internal class BrokerRegistry
{

    public static bool RegistryValueExists(string hive_HKLM_or_HKCU, string registryRoot, string valueName)
    {
        RegistryKey root;
        switch (hive_HKLM_or_HKCU.ToUpper())
        {
            case "HKLM":
                root = Registry.LocalMachine.OpenSubKey(registryRoot, false);
                break;
            case "HKCU":
                root = Registry.CurrentUser.OpenSubKey(registryRoot, false);
                break;
            default:
                throw new System.InvalidOperationException("parameter registryRoot must be either \"HKLM\" or \"HKCU\"");
        }
        if(root == null)
        {
            return false;
        }
        return root.GetValue(valueName) != null;
    }

    //public static string RegistryGetValue(string hive_HKLM_or_HKCU, string registryRoot, string valueName)
    //{
    //    RegistryKey root;
    //    switch (hive_HKLM_or_HKCU.ToUpper())
    //    {
    //        case "HKLM":
    //            root = Registry.LocalMachine.OpenSubKey(registryRoot, false);
    //            break;
    //        case "HKCU":
    //            root = Registry.CurrentUser.OpenSubKey(registryRoot, false);
    //            break;
    //        default:
    //            throw new System.InvalidOperationException("parameter registryRoot must be either \"HKLM\" or \"HKCU\"");
    //    }
    //    if(root == null) return string.Empty;
    //    return root.GetValue(valueName)?.ToString() ?? string.Empty;
    //}

    public static object? RegistryGetValue(string hive_HKLM_or_HKCU, string registryRoot, string valueName)
    {
        RegistryKey root;
        switch (hive_HKLM_or_HKCU.ToUpper())
        {
            case "HKLM":
                root = Registry.LocalMachine.OpenSubKey(registryRoot, false);
                break;
            case "HKCU":
                root = Registry.CurrentUser.OpenSubKey(registryRoot, false);
                break;
            default:
                throw new System.InvalidOperationException("parameter registryRoot must be either \"HKLM\" or \"HKCU\"");
        }
        if (root == null) return null;
        else
        return root.GetValue(valueName) ?? null;
    }

    public static void RegistrySetValue(string hive_HKLM_or_HKCU, string registryRoot, string valueName, object value, RegistryValueKind valueKind)
    {
        RegistryKey root;
        switch (hive_HKLM_or_HKCU.ToUpper())
        {
            case "HKLM":
                root = Registry.LocalMachine.CreateSubKey(registryRoot);
                break;
            case "HKCU":
                root = Registry.CurrentUser.CreateSubKey(registryRoot);
                break;
            default:
                throw new System.InvalidOperationException("parameter registryRoot must be either \"HKLM\" or \"HKCU\"");
        }
        if(root == null) return;
        root.SetValue(valueName, value, valueKind);
    }

}
