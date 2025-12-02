using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollViewNavigator : MonoBehaviour
{
    public ScrollRect scrollRect;
    private GameObject lastSelected;
    public float extraOffset;
    void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected != null && selected != lastSelected)
        {
            // Kolla om den nya selection är ett barn till scrollRect's content
            if (selected.transform.IsChildOf(scrollRect.content))
            {
                ScrollToSelected(selected.GetComponent<RectTransform>());
            }

            lastSelected = selected;
        }
    }

    private void ScrollToSelected(RectTransform target)
    {
        Canvas.ForceUpdateCanvases(); // Se till layouten är aktuell!

        RectTransform viewport = scrollRect.viewport;
        RectTransform content = scrollRect.content;

        Vector3[] viewportWorldCorners = new Vector3[4];
        Vector3[] targetWorldCorners = new Vector3[4];

        viewport.GetWorldCorners(viewportWorldCorners);
        target.GetWorldCorners(targetWorldCorners);

        float viewportTop = viewportWorldCorners[1].y;
        float viewportBottom = viewportWorldCorners[0].y;

        float targetTop = targetWorldCorners[1].y;
        float targetBottom = targetWorldCorners[0].y;

        float contentHeight = content.rect.height;
        float viewportHeight = viewport.rect.height;

        float scrollDelta = 0f;

        // Om target är ovanför viewport-toppen
        if (targetTop > viewportTop)
        {
            Debug.Log("target är ovanför");

            float offset = targetTop - viewportTop;
            float normalizedOffset = offset / (contentHeight - viewportHeight);
            scrollDelta = normalizedOffset + extraOffset;
        }
        // Om target är nedanför viewport-bottom
        else if (targetBottom < viewportBottom)
        {
            Debug.Log("target är under");
            float _extraOffset = extraOffset * -1;
            float offset = viewportBottom - targetBottom;
            float normalizedOffset = offset / (contentHeight - viewportHeight);
            scrollDelta = -normalizedOffset + _extraOffset;
        }

        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + scrollDelta);
    }
}