using System;
using UnityEngine;

public interface IUnitView3D : IDisposable
{
    void Init(UnitViewModel vm);
    void Play(UnitAnimationState state);
    void SetMaterial(Material mat);
}
