using UnityEngine;
using UnityEngine.EventSystems;

public static class PointerEventSimulator
{
    public static bool SimulatePointerEnter(GameObject g)
    {
        return SimulatePointerEvent<IPointerEnterHandler>(g, ExecuteEvents.pointerEnterHandler);
    }
    public static bool SimulatePointerExit(GameObject g)
    {
        return SimulatePointerEvent<IPointerExitHandler>(g, ExecuteEvents.pointerExitHandler);
    }
    public static bool SimulatePointerDown(GameObject g)
    {
        return SimulatePointerEvent<IPointerDownHandler>(g, ExecuteEvents.pointerDownHandler);
    }
    public static bool SimulatePointerUp(GameObject g)
    {
        return SimulatePointerEvent<IPointerUpHandler>(g, ExecuteEvents.pointerUpHandler);
    }
    public static bool SimulateClick(GameObject g)
    {
        return SimulatePointerEvent<IPointerClickHandler>(g, ExecuteEvents.pointerClickHandler);
    }

    public static bool SimulateSelect(GameObject g)
    {
        return SimulatePointerEvent<ISelectHandler>(g, ExecuteEvents.selectHandler);
    }
    public static bool SimulateDeselect(GameObject g)
    {
        return SimulatePointerEvent<IDeselectHandler>(g, ExecuteEvents.deselectHandler);
    }


    public static bool SimulatePointerEvent<T>(GameObject target, ExecuteEvents.EventFunction<T> functor) where T : IEventSystemHandler
    {
        return ExecuteEvents.Execute<T>(target, new PointerEventData(EventSystem.current), functor);
    }
}