using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.XR;
using System.Linq;
using UnityEngine.SpatialTracking;

public class ControllerScript : MonoBehaviour
{
    [SerializeField]
    private XRNode m_xrNode = XRNode.LeftHand;
    private List<InputDevice> m_devices = new List<InputDevice>();
    private InputDevice m_device;

    private GameObject lastHit = null;
    private float hitDuration = 0f;
    private bool interacted = false;
    [SerializeField]
    private Button button = null;

    [SerializeField]
    private float triggerTreshold = 0.1f;
    [SerializeField]
    private float triggerTime = 1f;
    [SerializeField]
    private bool repeatable = false;
    [SerializeField]
    private Canvas buttonCanvas = null;
    [SerializeField]
    private float indicatorScale = 1f;
    [SerializeField]
    private LineRenderer line = null;
    [SerializeField]
    private EventSystem events = null;

    [SerializeField]
    private float m_originalWidth = 0.5f;

    private enum LineStates { active, inactive, attempting}
    [SerializeField]
    private LineStates m_lineState = LineStates.inactive;

    public Vector3 m_pointerDirection = Vector3.down;

    void GetDevice()
    {
        if (GetComponentInParent<TrackedPoseDriver>().poseSource == TrackedPoseDriver.TrackedPose.LeftPose)
            m_xrNode = XRNode.LeftHand;
        else
            m_xrNode = XRNode.RightHand;

        InputDevices.GetDevicesAtXRNode(m_xrNode, m_devices);
        m_device = m_devices.FirstOrDefault();
    }

    private void Start()
    {
        if (!m_device.isValid)
            GetDevice();

        if (buttonCanvas == null)
            buttonCanvas=GetComponentInChildren<Canvas>();
        if (line == null)
            line = GetComponentInChildren<LineRenderer>();
        if (events == null)
            events = FindObjectOfType<EventSystem>();
        if (this.transform.parent.tag == "MainCamera")
            line.enabled = false;

        m_originalWidth = line.widthMultiplier;

        DisableLine();
    }

    void Update()
    {
        if (!m_device.isValid)
            GetDevice();

        switch (m_lineState)
        {
            case LineStates.active:
                CheckController();
                break;
            case LineStates.attempting:
                EnableLine();
                break;
        }

        if (m_lineState == LineStates.attempting)
            EnableLine();

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(m_pointerDirection), out hit, Mathf.Infinity))
        {
            UpdateLine(hit);
            float triggerAxis = 0f; //OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, m_controller);
            InputFeatureUsage<float> triggerUsage = CommonUsages.trigger;
            if (m_device.TryGetFeatureValue(triggerUsage, out triggerAxis) && (hit.transform.tag == "Button"))
            {
                button = hit.transform.gameObject.GetComponent<Button>();
                StartHover();

                if ((triggerAxis > triggerTreshold))
                {
                    ShowInteractionCircle();

                    UpdateInteractionCircle(hit);

                    hitDuration += 1 * Time.deltaTime;
                    UpdateInteractionCircle(hit);
                    if ((hitDuration > triggerTime) && (interacted == false))
                    {
                        Interact();
                        HideInteractionCirle();
                    }


                    if (lastHit != hit.transform.gameObject)
                    {
                        ShowInteractionCircle();
                        ResetInteraction();
                        UpdateInteractionCircle(hit);
                    }

                    lastHit = hit.transform.gameObject;
                }
                else
                {
                    ResetInteraction();
                    HideInteractionCirle();
                }
            }
            else
            {
                ResetInteraction();
                HideInteractionCirle();

                StopHover();
                button = null;
            }
        }
        else
        {
            ResetInteraction();
            HideInteractionCirle();
            ResetLine();
        }
    }

    void HideInteractionCirle()
    {
        if (buttonCanvas != null)
            buttonCanvas.enabled = false;
    }
    void ShowInteractionCircle()
    {
        if (buttonCanvas != null)
            buttonCanvas.enabled = true;
    }
    void UpdateInteractionCircle(RaycastHit hit)
    {
        if (buttonCanvas != null)
        {
            buttonCanvas.transform.position = hit.point;
            buttonCanvas.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
            float distance = Vector3.Distance(this.transform.position, hit.point);
            buttonCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(indicatorScale * distance, indicatorScale * distance);
            buttonCanvas.GetComponentInChildren<Image>().fillAmount = Mathf.Clamp((hitDuration / triggerTime), 0, 1);
        }
    }

    void UpdateLine(RaycastHit hit)
    {
        line.useWorldSpace = true;
        line.SetPosition(0, this.transform.position);
        line.SetPosition(1, hit.point);
    }

    public void EnableLine()
    {
        if (m_device.isValid)
        {
            line.widthMultiplier = m_originalWidth;
            m_lineState = LineStates.active;
        }
        else
        {
            m_lineState = LineStates.attempting;
            GetDevice();
        }
    }

    void CheckController()
    {
        GetDevice();
        if (!m_device.isValid)
        {
            ControllerLost();
        }
    }

    public void ControllerLost()
    {
        m_lineState = LineStates.attempting;
        line.widthMultiplier = 0f;
    }

    public void DisableLine()
    {
        m_lineState = LineStates.inactive;
        line.widthMultiplier = 0f;
    }

    void ResetLine()
    {
        line.useWorldSpace = false;
        line.SetPosition(0, new Vector3(0, 0, 0));
        line.SetPosition(1, m_pointerDirection * 1000);
    }

    void ResetInteraction()
    {
        StopHover();
        hitDuration = 0f;
        interacted = false;
        button = null;
        lastHit = null;
    }

    void Interact()
    {
        interacted = true;

        if (repeatable)
            ResetInteraction();

        if (button != null)
            button.onClick.Invoke();
    }

    void StartHover()
    {
        if (button != null)
            button.Select();
    }

    void StopHover()
    {
        events.SetSelectedGameObject(null);
    }

    public bool IsLineActive() { return m_lineState.Equals(LineStates.active); }
}
