using Firebase;
using Firebase.Analytics;
using TMPro;
using UnityEngine;

public class FirebaseInit : MonoBehaviour
{
    //public TMP_Text HelloWorld;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("FirebaseInit Start");
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
        });
        Debug.Log("FirebaseInit Setup analytics");
        //HelloWorld.text = "Set up analytics";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
