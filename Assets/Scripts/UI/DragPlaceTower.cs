using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class DragPlaceTower : MonoBehaviour
{

    [SerializeField] private int towerPrice = 50;

    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(1f, 1f, 1f, 0.4f);

    [SerializeField] private Color canPlaceColor = Color.white;
    [SerializeField] private Color placeRestrictedColor = Color.red;

    [SerializeField] private SpriteRenderer dragDropIcon = null;
    [SerializeField] private GameObject towerPrefab = null;
    [SerializeField] private NodeGrid grid = null;

    [SerializeField] private SpriteRenderer shopImage = null;

    [SerializeField] private PlayerState usingPlayer = null;

    private bool canAfford;

    private Vector2 dragDropHiddenPosition;
    

    private void Start()
    {
        dragDropHiddenPosition = dragDropIcon.transform.position;
        usingPlayer.MoneyChanged += OnMoneyChanged;
        OnMoneyChanged(usingPlayer.Money);
    }

    private void OnMoneyChanged(int money)
    {
        if (usingPlayer.Money >= towerPrice)
        {
            canAfford = true;
            shopImage.color = activeColor;
        }
        else
        {
            canAfford = false;
            shopImage.color = inactiveColor;
        }
    }

    private Coroutine activeDragRoutine;

    private void OnMouseDown()
    {
        if (canAfford)
            activeDragRoutine = StartCoroutine(DragTower());
    }

    private IEnumerator DragTower()
    {
        while (true)
        {
            // Snap the icon to the nearest grid unit.
            Vector2 location = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int unitsX = Mathf.FloorToInt((location.x - grid.transform.position.x) / grid.GridUnit);
            int unitsY = Mathf.FloorToInt((location.y - grid.transform.position.y) / grid.GridUnit);
            
            // Is the tower location within the valid tile region?
            if (unitsX >= 0 && unitsX < grid.Width
                && unitsY >= 0 && unitsY < grid.Height)
            {
                // Snap location to grid.
                dragDropIcon.transform.position = (Vector2)grid.transform.position
                + grid.GridUnit * new Vector2(unitsX + 0.5f, unitsY + 0.5f);
                // Check if the given tile is build legal.
                if (grid.IsTileBuildLegal(unitsX, unitsY))
                    dragDropIcon.color = canPlaceColor;
                else
                    dragDropIcon.color = placeRestrictedColor;
            }
            else
            {
                // If not, don't snap and show restricted placement.
                dragDropIcon.transform.position = location;
                dragDropIcon.color = placeRestrictedColor;
            }

            yield return null;
        }
    }

    private void OnMouseUp()
    {
        if (activeDragRoutine != null)
        {
            StopCoroutine(activeDragRoutine);
            activeDragRoutine = null;

            // Snap the icon to the nearest grid unit.
            Vector2 location = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            int unitsX = Mathf.FloorToInt((location.x - grid.transform.position.x) / grid.GridUnit);
            int unitsY = Mathf.FloorToInt((location.y - grid.transform.position.y) / grid.GridUnit);
            // Check to see if a tower should be placed.
            if (unitsX >= 0 && unitsX < grid.Width
                && unitsY >= 0 && unitsY < grid.Height
                && grid.IsTileBuildLegal(unitsX, unitsY))
            {
                GameObject newTower = Instantiate(towerPrefab);
                newTower.transform.position = (Vector2)grid.transform.position
                + grid.GridUnit * new Vector2(unitsX + 0.5f, unitsY + 0.5f);
                grid.MarkTileOccupied(unitsX, unitsY);
                usingPlayer.Money -= towerPrice;
                Tower tower = newTower.GetComponent<Tower>();
                tower.OnPlacement(grid);
            }
        }
        dragDropIcon.transform.position = dragDropHiddenPosition;
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
