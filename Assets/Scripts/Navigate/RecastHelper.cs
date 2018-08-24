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
    public static extern void release_map_bin(string path);
    [DllImport(dll_name)]
    public static extern int get_mesh_vert_count();

    [DllImport(dll_name)]
    public static extern void get_mesh_vert_pos(float[] vertPos);

    [DllImport(dll_name)]
    public static extern bool load_ob_bin(string path);

    [DllImport(dll_name)]
    public static extern void save_ob_bin(string path);

    [DllImport(dll_name)]
    public static extern int get_ob_box_count();

    [DllImport(dll_name)]
    public static extern int get_ob_boxs(out float[] bmin, out float[] bmax);

    [DllImport(dll_name)]
    public static extern void add_ob_boxs(float[] bmin, float[] bmax);

    [DllImport(dll_name)]
    public static extern void del_ob_boxs(float[] bmin, float[] bmax);

    [DllImport(dll_name)]
    public static extern void del_all_ob();

    [DllImport(dll_name)]
    public static extern bool is_valid_pos(float[] pos);

    [DllImport(dll_name)]
    public static extern void find_path(float[] start, float[] end, out float[] paths, out int pathCount);

    [DllImport(dll_name)]
    public static extern bool is_hit_ob(float[] start, float[] end);

    [DllImport(dll_name)]
    public static extern void raycast(float[] start, float[] end, out float[] hitPoint, out float[] hitNormal);

    [DllImport(dll_name)]
    public static extern void get_pos_height(float[] pos, out float height);
}
