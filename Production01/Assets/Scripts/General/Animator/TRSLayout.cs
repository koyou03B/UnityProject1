using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// AnimationをClipしてできたボーンの姿勢データ
/// ComputeBufferに渡してGPUで計算につかってもらう
/// T:Translation(位置) R:Rotation(回転) S:Scale(スケール)
/// 
/// StructLayout(LayoutKind.Sequential)とは
/// フィールドを宣言した時に勝手に入れ替えないHLSL側の構造体と
/// バイト単位で一致させるために必要(ズレることが命取りになるから)
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct TRSLayout
{
    public Vector3 _Position;
    public float _Dummy1;
    public Vector4 _Rotation;
    public Vector3 _Scale;
    public float _Dummy2;

    public static TRSLayout From(Vector3 p, Vector4 r, Vector3 s)
     => new TRSLayout { _Position = p, _Rotation = r, _Scale = s };
}
