using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractPart : MonoBehaviour {
    [SerializeField] private PartType type;
    [SerializeField] private SteamEngine engine;
    [SerializeField] private Handle handle;
    [SerializeField] private InteractPartAxis axis;

    private float _lastXValue;
    private float _lastYValue;

    private void Update() {
        if (handle.XValue == _lastXValue && handle.YValue == _lastYValue) return;

        switch (type) {
            case PartType.RegulatorValve:
                float value = Utils.Remap(GetSingleAxis(), -1, 1, 0, 100);
                engine.SetSpeedPercentage(value);
                break;
            case PartType.RegulatorGauge:
                break;
            case PartType.ReverseWheel:
                engine.SetTurn(GetSingleAxis());
                break;
            case PartType.WaterInjector:
                break;
            case PartType.BoilerPressureGauge:
                break;
            case PartType.WaterPurifier:
                break;
            case PartType.DirectionGauge:
                break;
        }

        _lastXValue = handle.XValue;
        _lastYValue = handle.YValue;
    }

    private float GetSingleAxis() {
        return axis == InteractPartAxis.XAxis ? handle.XValue : axis == InteractPartAxis.YAxis ? handle.YValue : 0;
    }
}

public enum PartType {
    RegulatorValve,
    RegulatorGauge,
    ReverseWheel,
    WaterInjector,
    BoilerPressureGauge,
    WaterPurifier,
    DirectionGauge,
}

public enum InteractPartAxis {
    XAxis,
    YAxis,
    Both
}
