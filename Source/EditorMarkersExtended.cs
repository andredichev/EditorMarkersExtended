using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using KSP.Localization;
using KSP.UI.TooltipTypes;

[KSPAddon(KSPAddon.Startup.EditorAny, false)]
public class EditorMarkersExtended : MonoBehaviour
{
    private LineRenderer cotLine;
    private LineRenderer[] comLines = new LineRenderer[3];
    private LineRenderer[] colLines = new LineRenderer[3];

    private const float RayLength = 50f;

    private EditorMarker_CoT cachedCoT;
    private EditorMarker_CoM cachedCoM;
    private EditorMarker_CoL cachedCoL;

    private Material sharedMaterial;

    private bool showExtendedCoT = true;
    private bool showExtendedCoM = true;
    private bool showExtendedCoL = true;
    private bool tooltipsModified = false;

    private Vector3 lastCotPos, lastCotDir;
    private Vector3 lastComPos;
    private Vector3 lastColPos;

    public void Start()
    {
        sharedMaterial = CreateXRayMaterial();
        cotLine = CreateLine(new Color(1f, 0f, 1f, 0.5f));

        for (int i = 0; i < 3; i++)
        {
            comLines[i] = CreateLine(new Color(1f, 1f, 0f, 0.5f));
            colLines[i] = CreateLine(new Color(0f, 1f, 1f, 0.5f));
        }

        StartCoroutine(FindMarkersRoutine());
    }

    public void Update()
    {
        if (EditorLogic.fetch == null) return;

        HandleRightClickToggles();
        UpdateCoTMarker();
        UpdateCoMMarker();
        UpdateCoLMarker();
    }

    private IEnumerator FindMarkersRoutine()
    {
        var wait = new WaitForSeconds(0.5f);

        while (true)
        {
            if (cachedCoT == null) cachedCoT = UnityEngine.Object.FindObjectOfType<EditorMarker_CoT>();
            if (cachedCoM == null) cachedCoM = UnityEngine.Object.FindObjectOfType<EditorMarker_CoM>();
            if (cachedCoL == null) cachedCoL = UnityEngine.Object.FindObjectOfType<EditorMarker_CoL>();

            if (!tooltipsModified && EditorLogic.fetch != null)
            {
                TryModifyTooltips();
            }

            yield return wait;
        }
    }

    private void TryModifyTooltips()
    {
        var controllers = UnityEngine.Object.FindObjectsOfType<TooltipController_Text>();
        if (controllers == null || controllers.Length == 0) return;

        string hintText = " / " + Localizer.Format("#LOC_EMExt_Tooltip_Hint");

        bool comFound = false;
        bool cotFound = false;
        bool colFound = false;

        foreach (var controller in controllers)
        {
            if (controller == null || string.IsNullOrEmpty(controller.textString)) continue;

            string name = controller.gameObject.name;

            if (!ContainsIgnoreCase(name, "btn") && !ContainsIgnoreCase(name, "button") && !ContainsIgnoreCase(name, "toggle"))
                continue;

            bool isCom = ContainsIgnoreCase(name, "com");
            bool isCot = ContainsIgnoreCase(name, "cot");
            bool isCol = ContainsIgnoreCase(name, "col");

            if (!isCom && !isCot && !isCol) continue;

            comFound |= isCom;
            cotFound |= isCot;
            colFound |= isCol;

            if (!controller.textString.Contains(hintText))
            {
                controller.textString = Localizer.Format(controller.textString) + hintText;
            }
        }

        tooltipsModified = comFound && cotFound && colFound;
    }

    private void HandleRightClickToggles()
    {
        if (!Input.GetMouseButtonDown(1) || EventSystem.current == null) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            for (Transform current = result.gameObject.transform; current != null; current = current.parent)
            {
                string name = current.name;

                if (ContainsIgnoreCase(name, "panel") || ContainsIgnoreCase(name, "canvas")) break;

                if (!ContainsIgnoreCase(name, "btn") && !ContainsIgnoreCase(name, "button") && !ContainsIgnoreCase(name, "toggle"))
                    continue;

                if (ContainsIgnoreCase(name, "com"))
                {
                    TryToggleMarker(ref cachedCoM, ref showExtendedCoM, "#LOC_EMExt_CoM_Msg");
                    return;
                }
                if (ContainsIgnoreCase(name, "cot"))
                {
                    TryToggleMarker(ref cachedCoT, ref showExtendedCoT, "#LOC_EMExt_CoT_Msg");
                    return;
                }
                if (ContainsIgnoreCase(name, "col"))
                {
                    TryToggleMarker(ref cachedCoL, ref showExtendedCoL, "#LOC_EMExt_CoL_Msg");
                    return;
                }
            }
        }
    }

    private void TryToggleMarker<T>(ref T markerCache, ref bool showExtended, string locTag) where T : MonoBehaviour
    {
        if (markerCache == null) markerCache = UnityEngine.Object.FindObjectOfType<T>();

        if (markerCache != null && markerCache.gameObject.activeInHierarchy)
        {
            showExtended = !showExtended;
            string stateTag = showExtended ? "#LOC_EMExt_On" : "#LOC_EMExt_Off";
            ScreenMessages.PostScreenMessage(Localizer.Format(locTag, Localizer.Format(stateTag)), 2.0f, ScreenMessageStyle.LOWER_CENTER);
        }
    }

    private void UpdateCoTMarker()
    {
        bool shouldShow = showExtendedCoT && cachedCoT != null && cachedCoT.gameObject.activeInHierarchy;

        if (!shouldShow)
        {
            if (cotLine.enabled) cotLine.enabled = false;
            return;
        }

        Vector3 pos = EditorMarker_CoT.Pos;
        Vector3 dir = EditorMarker_CoT.Dir;

        if (cotLine.enabled && pos == lastCotPos && dir == lastCotDir) return;

        lastCotPos = pos;
        lastCotDir = dir;

        if (!cotLine.enabled) cotLine.enabled = true;

        cotLine.SetPosition(0, pos - dir * RayLength);
        cotLine.SetPosition(1, pos + dir * RayLength);
    }

    private void UpdateCoMMarker()
    {
        UpdateAxisLines(cachedCoM, comLines, showExtendedCoM, ref lastComPos);
    }

    private void UpdateCoLMarker()
    {
        UpdateAxisLines(cachedCoL, colLines, showExtendedCoL, ref lastColPos);
    }

    private void UpdateAxisLines(MonoBehaviour marker, LineRenderer[] lines, bool showExtended, ref Vector3 lastPos)
    {
        bool shouldShow = showExtended && marker != null && marker.gameObject.activeInHierarchy;

        if (!shouldShow)
        {
            if (lines[0].enabled) ToggleLines(lines, false);
            return;
        }

        Vector3 pos = marker.transform.position;

        if (lines[0].enabled && pos == lastPos) return;

        lastPos = pos;

        if (!lines[0].enabled) ToggleLines(lines, true);

        lines[0].SetPosition(0, pos - Vector3.right * RayLength);
        lines[0].SetPosition(1, pos + Vector3.right * RayLength);

        lines[1].SetPosition(0, pos - Vector3.up * RayLength);
        lines[1].SetPosition(1, pos + Vector3.up * RayLength);

        lines[2].SetPosition(0, pos - Vector3.forward * RayLength);
        lines[2].SetPosition(1, pos + Vector3.forward * RayLength);
    }

    private void ToggleLines(LineRenderer[] lines, bool state)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null) lines[i].enabled = state;
        }
    }

    private bool ContainsIgnoreCase(string source, string target)
    {
        return source.IndexOf(target, StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private Material CreateXRayMaterial()
    {
        Material mat = new Material(Shader.Find("Hidden/Internal-Colored"));
        mat.SetInt("_ZTest", 8);
        mat.SetInt("_ZWrite", 0);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        return mat;
    }

    private LineRenderer CreateLine(Color color)
    {
        GameObject lineObj = new GameObject("ExtendedMarkerLine");
        lineObj.transform.SetParent(this.transform);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = sharedMaterial;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.positionCount = 2;
        lr.enabled = false;

        return lr;
    }

    private void OnDestroy()
    {
        if (sharedMaterial != null) Destroy(sharedMaterial);
        if (cotLine != null)        Destroy(cotLine.gameObject);

        for (int i = 0; i < 3; i++)
        {
            if (comLines[i] != null) Destroy(comLines[i].gameObject);
            if (colLines[i] != null) Destroy(colLines[i].gameObject);
        }
    }
}
