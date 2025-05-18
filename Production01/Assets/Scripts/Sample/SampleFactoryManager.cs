using System.Threading.Tasks;
using UnityEngine;

public class SampleFactoryManager : MonoBehaviour
{
    [SerializeField]
    private SampleBoxFactory  _Sample;

    GameObject[] pool;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async Task Start()
    {
        await _Sample.Initialize();
    
    }

    // Update is called once per frame
    void Update()
    {
     if(InputSystemController.Instance.GetKeyDown(InputSystemKeyCode.eInputSystemKeyCode.A))
        {
            pool = new GameObject[5];
            for (int i = 0; i < 5;i++)
            {
                pool [i]= _Sample.ObjectPool.Get();
            }
        }
        else if (InputSystemController.Instance.GetKeyDown(InputSystemKeyCode.eInputSystemKeyCode.B))
        {
            for (int i = 0; i < 5; i++)
            {
               _Sample.ObjectPool.Release(pool[i]);
            }
        }
    }
}
