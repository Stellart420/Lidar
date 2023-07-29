using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class VirtonexRayInteractor : XRRayInteractor
{
    [SerializeField] private Transform XRRig;

    private LocomotionManager _locomotionManager;
    private ActionBasedContinuousMoveProvider _actionBasedContinuousMoveProvider;
    private ActionBasedContinuousTurnProvider _actionBasedContinuousTurnProvider;
    private ActionBasedSnapTurnProvider _actionBasedSnapTurnProvider;

    private Dictionary<string, bool> _componentsStatus;

    protected override void Start()
    {
        base.Start();

        if (XRRig != null)
        {
            _locomotionManager = XRRig.GetComponent<LocomotionManager>();
            _actionBasedContinuousMoveProvider = XRRig.GetComponent<ActionBasedContinuousMoveProvider>();
            _actionBasedContinuousTurnProvider = XRRig.GetComponent<ActionBasedContinuousTurnProvider>();
            _actionBasedSnapTurnProvider = XRRig.GetComponent<ActionBasedSnapTurnProvider>();
        }
    }
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        _componentsStatus = new Dictionary<string, bool>()
        {
            {"locomotion manager", _locomotionManager.enabled },
            {"continious move provider", _actionBasedContinuousMoveProvider.enabled },
            {"continious turn provider", _actionBasedContinuousTurnProvider.enabled },
            {"snap turn provider", _actionBasedSnapTurnProvider.enabled }
        };

        // Disable movement
        if (XRRig != null)
        {
            _locomotionManager.enabled = false;
            _actionBasedContinuousMoveProvider.enabled = false;
            _actionBasedContinuousTurnProvider.enabled = false;
            _actionBasedSnapTurnProvider.enabled = false;
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // Enable movement
        if (XRRig != null)
        {
            _locomotionManager.enabled = _componentsStatus["locomotion manager"];
            _actionBasedContinuousMoveProvider.enabled = _componentsStatus["continious move provider"];
            _actionBasedContinuousTurnProvider.enabled = _componentsStatus["continious turn provider"];
            _actionBasedSnapTurnProvider.enabled = _componentsStatus["snap turn provider"];
        }
    }
}
