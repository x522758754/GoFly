using System.Runtime.InteropServices;
using UnityEngine;

//C++ Recast提供的借口封装
public class RecastHelper
{
#if UNITY_IPHONE
    /// On iOS plugins are statically linked into the executable, so we have to use __Internal as the library name.
    const string dll_name = "__Internal";    
#else
    /// Other platforms load plugins dynamically, so pass the name of the plugin's dynamic library.
    /// 64bit dll
    const string dll_name = "RecastUnity";
#endif

    [DllImport(dll_name)]
    public static extern bool load_map_bin(string path);
    [DllImport(dll_name)]
    public static extern int get_tri_vert_count();

    [DllImport(dll_name)]
    public static extern void get_tri_vert_pos(float[] vertPos);

    [DllImport(dll_name)]
    public static extern bool load_ob_bin(string path);

    [DllImport(dll_name)]
    public static extern int get_ob_count();

    [DllImport(dll_name)]
    public static extern int get_ob_info(float[] obPos);
}
