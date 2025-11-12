using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.UI.Image;
using static GridSystem;

public class GridDragController : MonoBehaviour
{
    //legacy input co the refactor sau
    [SerializeField]private GridSystem gridSystem;
    [SerializeField] private GameObject testObject;
    private Grid<CarObject> grid;
    private int startX, startZ;
    private float cellSize;
    private float dragThreshold;
    private Vector3 startMousePos;
    private Vector3 currentMousePos;
    private Vector3 endMousePos;
    private bool isDragging;
    private bool isMoving;
    private float totalWidth;
    private int maxShift;
    private int width;
    private int height;
    private readonly int firstRowMove = 2;
    private readonly int secondRowMove = 1;
    private int rowToMove;
    private void Start()
    {
        gridSystem.OnLoadDataSuccess += GridSystem_OnLoadDataSuccess;
    }

    private void GridSystem_OnLoadDataSuccess()
    {
        Init();
    }

    private void Init()
    {
        grid = gridSystem.GetCarGrid();
        isDragging = false;
        isMoving = false;
        cellSize = grid.GetCellSize();
        totalWidth = grid.GetTotalWidth();
        width = grid.GetGridWidth();
        height = grid.GetGridHeight();
        dragThreshold = 0.3f;
        maxShift = Mathf.RoundToInt(totalWidth / cellSize);
    }


    private void Update()
    {
        //DRAG
        if (gridSystem.isProcessing)
        {
            isDragging = false;
            return;
        }
        if (Input.GetMouseButtonDown(0))
        {
            startMousePos = Mouse3D.GetMouseWorldPosition();
            grid.GetXZ(startMousePos, out startX, out startZ);
            if(startX >=0 && startX<width && startZ<height && startZ>=1)
            {
                //gridSystem.GetRowVisualGroup(startZ).RememberOriginalPos();
                rowToMove = Mathf.Clamp(startZ, secondRowMove, firstRowMove);
                rowToMove = startZ;
                isDragging = true;
            }
        }

        if(isDragging)
        {
            currentMousePos = Mouse3D.GetMouseWorldPosition();
            float delta = currentMousePos.x - startMousePos.x;
            delta = Mathf.Clamp(delta, -totalWidth, totalWidth);
            gridSystem.GetRowVisualGroup(rowToMove).SetRowOffset(delta);
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            endMousePos = Mouse3D.GetMouseWorldPosition();
            Vector3 dragVector = endMousePos - startMousePos;
            if (dragVector.magnitude > dragThreshold)
            {
                int shiftCount = 0;
                shiftCount = Mathf.RoundToInt(dragVector.x / cellSize);
                shiftCount = Mathf.Clamp(shiftCount,-maxShift,maxShift);
                if (shiftCount > 0)
                {
                    for (int i = 0; i < shiftCount; i++)
                    {
                        grid.ShiftRowRight(rowToMove);
                        //Debug.Log($"Shift row {startZ} → {shiftCount} steps Right");
                    }
                }
                else if (shiftCount < 0)
                {
                    for (int i = 0; i < -shiftCount; i++)
                    {
                        grid.ShiftRowLeft(rowToMove);
                        //Debug.Log($"Shift row {startZ} ← {-shiftCount} steps Left");
                    }
                }
            }
            gridSystem.GetRowVisualGroup(rowToMove).ResetPosition();
            if(rowToMove == firstRowMove)
            {
                StartCoroutine(gridSystem.CheckDepartAndMove());
            }
        }

    }
}
