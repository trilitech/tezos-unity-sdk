using UnityEngine;

public class ExampleFactory : MonoBehaviour
{
    public static ExampleFactory Instance;
    private IExampleManager _exampleManager = null;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if(Instance != this)
            Destroy(this);
        
        _exampleManager = new ExampleManager();
        _exampleManager.Init();
    }

    public IExampleManager GetExampleManager()
    {
        return _exampleManager;
    }
}
