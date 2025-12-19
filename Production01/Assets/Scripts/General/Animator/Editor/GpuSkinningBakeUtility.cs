#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class GpuSkinningBakeUtility
{
    public static void BakeIntoAsset(
        GpuSkinningBakeAsset bakeAsset,
        SkinnedMeshRenderer sourceSMR,
        AnimationClip clip,
        int sampleRate // 30 or 60
    )
    {
        if (bakeAsset == null) throw new ArgumentNullException(nameof(bakeAsset));
        if (sourceSMR == null) throw new ArgumentNullException(nameof(sourceSMR));
        if (clip == null) throw new ArgumentNullException(nameof(clip));
        if (sampleRate <= 0) throw new ArgumentOutOfRangeException(nameof(sampleRate));

        //入力取り出し
        var mesh = sourceSMR.sharedMesh;
        if (mesh == null) throw new InvalidOperationException("SkinnedMeshRenderer.sharedMesh が null");

        Transform[] bones = sourceSMR.bones;
        if (bones == null || bones.Length == 0) throw new InvalidOperationException("SkinnedMeshRenderer.bones が空");

        var bindposesInv = mesh.bindposes;
        if (bindposesInv == null || bindposesInv.Length != bones.Length)
        {
            throw new InvalidOperationException(
                $"mesh.bindposes.Length({bindposesInv?.Length ?? 0}) != bones.Length({bones.Length})"
            );
        }

        int boneCount = bones.Length;
        // clipの長さ
        float clipLen = clip.length;
        //フレームカウント
        int frameCount = Mathf.Max(1, Mathf.FloorToInt(clipLen * sampleRate) + 1);
        //parentIndexの作成
        int[] parentIndex = BuildParentIndexArray(bones);
        //TRS配列の確保
        var clipTRS = new TRSLayout[frameCount * boneCount];

        //安全なサンプリングのため、対象を複製してAnimationModeで回す
        var tempRoot = UnityEngine.Object.Instantiate(sourceSMR.gameObject);
        tempRoot.name = sourceSMR.gameObject.name + "_BAKE_TEMP";
        tempRoot.hideFlags = HideFlags.HideAndDontSave;

        try
        {
            // temp側のSMRとbonesを取り直す(複製したので参照が変わる)
            var tempSMR = tempRoot.GetComponentInChildren<SkinnedMeshRenderer>();
            if (!tempSMR) throw new InvalidOperationException("複製側にSkinnedMeshRendererが見つからない");

            Transform[] tempBones = tempSMR.bones;
            if (tempBones == null || tempBones.Length != boneCount)
                throw new InvalidOperationException("複製側bonesが不正（boneCount不一致）");

            AnimationMode.StartAnimationMode();

            // フレームごとにサンプル
            for (int f = 0; f < frameCount; f++)
            {
                float t = f / (float)sampleRate;
                if (t > clipLen) t = clipLen;

                // AnimationModeでサンプリング
                AnimationMode.SampleAnimationClip(tempRoot, clip, t);

                // bones順でLocal TRSを取る
                for (int b = 0; b < boneCount; b++)
                {
                    var tr = tempBones[b];
                    Vector3 pos = tr.localPosition;
                    Vector4 rot = new Vector4(
                        tr.localRotation.x,
                        tr.localRotation.y,
                        tr.localRotation.z,
                        tr.localRotation.w
                    ); 
                    Vector3 scl = tr.localScale;

                    int index = f * boneCount + b;
                    clipTRS[index] = TRSLayout.From(pos, rot, scl);
                }
            }
        }
        finally
        {
            AnimationMode.StopAnimationMode();
            UnityEngine.Object.DestroyImmediate(tempRoot);
        }

        //BakeAssetへ書き込み
        WriteToAsset(bakeAsset,boneCount,frameCount,sampleRate,
            clipLen, parentIndex,bindposesInv,clipTRS);

        EditorUtility.SetDirty(bakeAsset);
        AssetDatabase.SaveAssets();
    }

    /// <summary>
    /// 親のIndexを取得する
    /// </summary>
    /// <param name="bones"></param>
    /// <returns></returns>
    private static int[] BuildParentIndexArray(Transform[] bones)
    {
        int length = bones.Length;
        var dict = new Dictionary<Transform, int>(length);
        for (int i = 0; i < length; i++)
        {
            dict[bones[i]] = i;
        }
        int[] parents = new int[length];
        for (int i = 0; i < length; i++)
        {
            Transform p = bones[i] ? bones[i].parent : null;
            if (p != null && dict.TryGetValue(p, out int pi))
            {
                parents[i] = pi;
            }
            else
            {
                parents[i] = -1;
            }
        }
        return parents;
    }

    /// <summary>
    /// GpuSkinningBakeAssetに情報を書き込む
    /// </summary>
    /// <param name="asset"></param>
    /// <param name="boneCount"></param>
    /// <param name="frameCount"></param>
    /// <param name="sampleRate"></param>
    /// <param name="clipLengthSec"></param>
    /// <param name="parentIndexArray"></param>
    /// <param name="bindposeInvArray"></param>
    /// <param name="clipTRSArray"></param>
    private static void WriteToAsset(
        GpuSkinningBakeAsset asset,
        int boneCount,
        int frameCount,
        int sampleRate,
        float clipLengthSec,
        int[] parentIndexArray,
        Matrix4x4[] bindposeInvArray,
        TRSLayout[] clipTRSArray
    )
    {
        asset.SetBakeDataEdit(
            boneCount,
            frameCount,
            sampleRate,
            clipLengthSec,
            parentIndexArray,
            bindposeInvArray,
            clipTRSArray
        );
    }
}
#endif
