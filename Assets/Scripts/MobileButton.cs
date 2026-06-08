using UnityEngine;
using UnityEngine.EventSystems;

public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum Direction { Up, Down, Left, Right }

    [SerializeField] private Direction direction;
    [SerializeField] private PlayerController player;

    public void OnPointerDown(PointerEventData eventData)
    {
        SetPressed(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetPressed(false);
    }

    private void SetPressed(bool pressed)
    {
        switch (direction)
        {
            case Direction.Up:    player.SetUp(pressed);    break;
            case Direction.Down:  player.SetDown(pressed);  break;
            case Direction.Left:  player.SetLeft(pressed);  break;
            case Direction.Right: player.SetRight(pressed); break;
        }
    }
}
