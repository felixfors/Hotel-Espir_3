using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationController : MonoBehaviour
{
    public static NotificationController instance;

    public bool test;
    public string testText;


    public int poolSize;
    public GameObject notificationPrefab;
    public List<Color> colorPallet = new List<Color>();
    public List<Notification> notification = new List<Notification>();

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        CreateNotificationPool();        
    }
    private void Update()
    {
        if(test)
        {
            NewNotification(testText);
            test = false;
        }
    }
    private void CreateNotificationPool()
    { 
        for(int i = 0; i < poolSize; i ++)
        {
            GameObject notificaitonObject = Instantiate(notificationPrefab, transform);
            Notification notificationComponent = notificaitonObject.GetComponent<Notification>();
            notificationComponent.canvasGroup.alpha = 0f;
            notification.Add(notificationComponent);

        }
    }

    public void NewNotification(string newNotificationText)
    {
        for(int i = 0; i < poolSize; i ++)
        {
            if(notification[i].canvasGroup.alpha == 0) // found available notification
            {
                notification[i].transform.SetAsLastSibling();
                notification[i].SetText(newNotificationText);
                notification[i].canvasGroup.alpha = 1;
                break;
            }
        }
    }

    public void ClearNotifications()
    {
        foreach(Notification _notification in notification)
        {
            _notification.CancelInvoke();
            _notification.canvasGroup.alpha = 0;
        }
    }


}
