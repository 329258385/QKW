using UnityEngine;
using UnityEngine.EventSystems;





class AddEventMonoCube
{
    public GameObject targetGameObject;


    void Start()
    {

    }


    public void AddObjectClickEvent( GameObject itemObject )
    {
        EventTrigger trigger = itemObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = itemObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        if( entry != null )
        {
            entry.eventID = EventTriggerType.PointerClick;
            UnityEngine.Events.UnityAction<BaseEventData> click = new UnityEngine.Events.UnityAction<BaseEventData>(OnClickCubeItem);
            entry.callback.AddListener(click);

            trigger.triggers.Clear();
            trigger.triggers.Add(entry);
        }
    }

    public void OnClickCubeItem(UnityEngine.EventSystems.BaseEventData data = null)
    {
        Debug.Log("点击了cube tran=");
    }

}
