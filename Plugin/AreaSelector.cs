using System;
using Assets.Behaviour.Flow;
using Assets.Behaviour.UI;
using Assets.Behaviour.Util;
using Assets.Source.Flow;
using Assets.Source.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using GameInput = Assets.Behaviour.Input.PlayerInput;

namespace KingdomHDSnapshot
{
    public sealed class AreaSelector : MonoBehaviour, IDragDropTarget
    {
        HDSnapshotPlugin owner;
        RectTransform rectangle;
        Image fillImage;
        Image topEdge, bottomEdge, leftEdge, rightEdge;
        GameObject infoBadge;
        TMP_Text infoText;

        Vector2Int anchor;
        Vector2Int cursor;
        bool dragging;

        public bool Active { get; private set; }

        public void Initialize(HDSnapshotPlugin plugin, RectTransform nativeTemplate)
        {
            owner = plugin;
            rectangle = (RectTransform)transform;

            if (nativeTemplate != null)
            {
                rectangle.anchorMin = nativeTemplate.anchorMin;
                rectangle.anchorMax = nativeTemplate.anchorMax;
                rectangle.pivot = new Vector2(0, 1); // Top-left pivot
                rectangle.localScale = Vector3.one;
                rectangle.localRotation = Quaternion.identity;
                rectangle.anchoredPosition3D = Vector3.zero;
            }

            // Semi-transparent blueprint blue / cyan fill
            fillImage = gameObject.AddComponent<Image>();
            fillImage.raycastTarget = false;
            fillImage.color = new Color(0.2f, 0.55f, 0.95f, 0.12f);

            // Crisp colored borders
            Color borderColor = new Color(0.35f, 0.75f, 1f, 0.95f);
            topEdge = CreateEdge("Top", new Vector2(0, 1), Vector2.one, new Vector2(0, 2), borderColor);
            bottomEdge = CreateEdge("Bottom", Vector2.zero, new Vector2(1, 0), new Vector2(0, 2), borderColor);
            leftEdge = CreateEdge("Left", Vector2.zero, new Vector2(0, 1), new Vector2(2, 0), borderColor);
            rightEdge = CreateEdge("Right", new Vector2(1, 0), Vector2.one, new Vector2(2, 0), borderColor);

            // Floating dimensions badge
            CreateInfoBadge();

            gameObject.SetActive(false);
        }

        Image CreateEdge(string name, Vector2 min, Vector2 max, Vector2 size, Color color)
        {
            var edge = new GameObject(name, typeof(RectTransform), typeof(Image));
            edge.transform.SetParent(transform, false);
            var rect = (RectTransform)edge.transform;
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            var image = edge.GetComponent<Image>();
            image.raycastTarget = false;
            image.color = color;
            return image;
        }

        void CreateInfoBadge()
        {
            infoBadge = new GameObject("InfoBadge", typeof(RectTransform), typeof(Image));
            infoBadge.transform.SetParent(transform, false);
            var bgRect = (RectTransform)infoBadge.transform;
            bgRect.pivot = new Vector2(0, 0);
            bgRect.anchorMin = new Vector2(0, 1);
            bgRect.anchorMax = new Vector2(0, 1);
            bgRect.anchoredPosition = new Vector2(4, 4);
            bgRect.sizeDelta = new Vector2(190, 26);

            var bg = infoBadge.GetComponent<Image>();
            bg.raycastTarget = false;
            bg.color = new Color(0.1f, 0.12f, 0.16f, 0.9f);

            var textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(infoBadge.transform, false);
            var textRect = (RectTransform)textObj.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            infoText = textObj.GetComponent<TextMeshProUGUI>();
            infoText.fontSize = 13;
            infoText.color = new Color(0.9f, 0.95f, 1f, 1f);
            infoText.alignment = TextAlignmentOptions.Center;
            infoText.text = "0 x 0";
        }

        public void Begin()
        {
            if (!WorldFlowManager.Visible || HDCaptureEngine.IsCapturing) return;

            Active = true;
            dragging = false;
            RefreshCursor();
            anchor = cursor;
            rectangle.sizeDelta = Vector2.zero;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();

            if (owner != null)
            {
                owner.SetToolActive(true);
            }

            // Register with DragDropManager so the game stops camera map-panning on left-click drag
            if (DragDropManager.Instance != null)
            {
                if (!DragDropManager.Instance.SetDragDropTarget(this, false))
                {
                    Cancel();
                }
            }
        }

        void Update()
        {
            if (!Active || HDCaptureEngine.IsCapturing) return;

            if (!WorldFlowManager.Visible || GameUI.MenuVisible)
            {
                Cancel();
                return;
            }

            // Cancel on Escape or Right click
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Cancel();
                return;
            }

            if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                Cancel();
                return;
            }

            // Shortcut: Space / Enter to Capture Entire Factory
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                CaptureAll();
                return;
            }

            // Start dragging when mouse is clicked
            if (!dragging && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                foreach (var ui in UIHelper.GetAllMouseOverUI())
                {
                    if (!ui.GetComponentInParent<WorldFlowElement>() &&
                        !ui.GetComponentInParent<WorldFlowDeco>() &&
                        !ui.GetComponentInParent<WorldFlowPipe>())
                    {
                        return;
                    }
                }

                RefreshCursor();
                anchor = cursor;
                dragging = true;
            }
        }

        void LateUpdate()
        {
            if (!Active || HDCaptureEngine.IsCapturing) return;

            if (dragging)
            {
                PanAtScreenEdge();
                RefreshCursor();
                UpdateRectangle();
            }
        }

        void RefreshCursor()
        {
            if (GameInput.Instance) GameInput.Instance.UpdateMouseWorld();
            cursor = GameInput.MouseWorld;
        }

        void UpdateRectangle()
        {
            var bounds = new SelectionBox(anchor.x, anchor.y, cursor.x, cursor.y);
            rectangle.anchoredPosition = new Vector2(bounds.Left, bounds.Top);
            rectangle.sizeDelta = new Vector2(bounds.Width, bounds.Height);

            if (infoText != null && owner != null)
            {
                float scale = owner.ResolutionScale;
                int pxW = Mathf.RoundToInt(bounds.Width * scale);
                int pxH = Mathf.RoundToInt(bounds.Height * scale);
                infoText.text = $"{bounds.Width}x{bounds.Height} ({pxW}x{pxH} px)";
            }
        }

        void PanAtScreenEdge()
        {
            var mouse = Mouse.current;
            if (!Application.isFocused || mouse == null || !WorldFlowCamera.Instance || !Camera.main) return;

            Vector2 mousePos = mouse.position.ReadValue();
            float margin = Mathf.Clamp(Screen.height * 0.04f, 28, 72);

            float dirX = 0f;
            if (mousePos.x < margin) dirX = -1f;
            else if (mousePos.x > Screen.width - margin) dirX = 1f;

            float dirY = 0f;
            if (mousePos.y < margin) dirY = -1f;
            else if (mousePos.y > Screen.height - margin) dirY = 1f;

            if (dirX != 0f || dirY != 0f)
            {
                float unitsPerPixel = 2f * Camera.main.orthographicSize / Mathf.Max(1f, Screen.height);
                Vector2 move = new Vector2(dirX, dirY).normalized * (500f * unitsPerPixel * Mathf.Min(Time.unscaledDeltaTime, 0.05f));
                WorldFlowCamera.Instance.Move(move.x, move.y);
            }
        }

        #region IDragDropTarget Implementation

        public Vector2Int GetGrabOffset(Vector2Int mousePosition) => Vector2Int.zero;

        public void OnDragDropMove(Vector2Int mousePosition, Vector2Int grabOffset)
        {
            cursor = mousePosition;
            if (!dragging) return;
            UpdateRectangle();
        }

        public void OnDragDropEnd()
        {
            if (Mouse.current == null || !Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Cancel();
                return;
            }

            if (dragging) RefreshCursor();

            var bounds = new SelectionBox(anchor.x, anchor.y, cursor.x, cursor.y);

            if (!dragging || !bounds.HasArea)
            {
                dragging = false;
                rectangle.sizeDelta = Vector2.zero;
                anchor = cursor = Vector2Int.zero;
                if (Active && DragDropManager.Instance != null)
                {
                    DragDropManager.Instance.SetDragDropTarget(this, false);
                }
                else
                {
                    Cancel();
                }
                return;
            }

            Cancel();
            owner.StartCapture(bounds);
        }

        public void OnDragDropCancel()
        {
            Cancel();
        }

        #endregion

        public void CaptureAll()
        {
            var flow = GamePlayer.Current?.ActiveFlow;
            if (flow == null || WorldFlowManager.Instance == null)
            {
                Cancel();
                return;
            }

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            bool foundAny = false;

            foreach (var elem in WorldFlowManager.Instance.Elements)
            {
                if (elem == null || !elem.isActiveAndEnabled) continue;
                foundAny = true;
                minX = Math.Min(minX, elem.Position.x);
                maxX = Math.Max(maxX, elem.Position.x + Mathf.RoundToInt(elem.Size.x));
                minY = Math.Min(minY, elem.Position.y - Mathf.RoundToInt(elem.Size.y));
                maxY = Math.Max(maxY, elem.Position.y);
            }

            if (flow.Decorations != null)
            {
                foreach (var deco in flow.Decorations)
                {
                    foundAny = true;
                    minX = Math.Min(minX, deco.Position.x);
                    maxX = Math.Max(maxX, deco.Position.x + deco.Size.x);
                    minY = Math.Min(minY, deco.Position.y - deco.Size.y);
                    maxY = Math.Max(maxY, deco.Position.y);
                }
            }

            if (!foundAny)
            {
                owner?.ShowNotification("No machines or decorations found to capture.");
                Cancel();
                return;
            }

            // Add 64 units (1 grid block) margin
            var bounds = new SelectionBox(minX - 64, minY - 64, maxX + 64, maxY + 64);
            Cancel();
            owner.StartCapture(bounds);
        }

        public void Cancel()
        {
            if (DragDropManager.Instance != null && ReferenceEquals(DragDropManager.Instance.Target, this))
            {
                DragDropManager.Instance.ClearDragDrop();
            }

            Active = false;
            dragging = false;
            gameObject.SetActive(false);

            if (owner != null)
            {
                owner.SetToolActive(false);
            }
        }

        void OnDisable()
        {
            if (DragDropManager.Instance != null && ReferenceEquals(DragDropManager.Instance.Target, this))
            {
                DragDropManager.Instance.ClearDragDrop();
            }
            Active = false;
            dragging = false;
        }

        void OnDestroy()
        {
            if (DragDropManager.Instance != null && ReferenceEquals(DragDropManager.Instance.Target, this))
            {
                DragDropManager.Instance.ClearDragDrop();
            }
        }

        void OnApplicationFocus(bool focused)
        {
            if (!focused && Active) Cancel();
        }
    }
}
