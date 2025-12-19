using UnityEngine;

[CreateAssetMenu(fileName = "GpuSkinningBakeAsset", menuName = "Scriptable Objects/GpuSkinningBakeAsset")]
public class GpuSkinningBakeAsset : ScriptableObject
{
    [SerializeField]
    //meshのボーンの数
    private int _BoneCount;
    [SerializeField]
    //アニメーションフレームの数
    private int _FrameCount;
    [SerializeField]
    //再生時の基準FPS(30or60で固定)
    private int _SampleRate;
    [SerializeField]
    //アニメーションクリップが何秒か
    private float _ClipLengthSec;
    [SerializeField]
    //それぞれのボーンの親のIndex
    private int[] _ParentIndexArray;
    [SerializeField]
    //BindPose(TポーズorAポーズ)時のボーン姿勢(空間を揃えるために逆行列が入っている)
    private Matrix4x4[] _BindposeInvArray;
    [SerializeField]
    //ClipしたLocal空間のボーン姿勢
    //長さ : _FrameCount*_BoneCount
    //並び : FrameMajor(frame * boneCount + boneでIndexを求める)
    private TRSLayout[] _ClipLocalTRS;

    public int BoneCount { get { return _BoneCount; } }
    public int FrameCount { get { return _FrameCount; }}
    public int SampleRate { get { return _SampleRate; }}
    public float ClipLengthSec { get { return _ClipLengthSec; }}
    public int[] ParentIndexArray { get { return _ParentIndexArray; }}
    public Matrix4x4[] BindposeInv { get { return _BindposeInvArray; } }
    public TRSLayout[] ClipLocalTRS { get { return _ClipLocalTRS; } }


#if UNITY_EDITOR

    /// <summary>
    /// Editor専用データ作成
    /// </summary>
    /// <param name="boneCount"></param>
    /// <param name="frameCount"></param>
    /// <param name="sampleRate"></param>
    /// <param name="clipLengthSec"></param>
    /// <param name="parentIndexArray"></param>
    /// <param name="bindposeInvArray"></param>
    /// <param name="clipTRSArray"></param>
    public void SetBakeDataEdit(
        int boneCount,
        int frameCount,
        int sampleRate,
        float clipLengthSec,
        int[] parentIndexArray,
        Matrix4x4[] bindposeInvArray,
        TRSLayout[] clipTRSArray
    )
    {
        _BoneCount = boneCount;
        _FrameCount = frameCount;
        _SampleRate = sampleRate;
        _ClipLengthSec = clipLengthSec;

        _ParentIndexArray = parentIndexArray;
        _BindposeInvArray = bindposeInvArray;
        _ClipLocalTRS = clipTRSArray;
    }
#endif
}
