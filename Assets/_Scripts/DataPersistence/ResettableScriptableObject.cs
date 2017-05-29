using System;
using UnityEngine;
using System.Collections.Generic;

// Instance of this class can be created as assets.
// Each instance contains collections of data from
// the Saver monobehaviours they have been referenced
// by.  Since assets exist outside of the scene, the
// data will persist ready to be reloaded next time
// the scene is loaded.  Note that these assets
// DO NOT persist between loads of a build and can
// therefore NOT be used for saving the gamestate to
// disk.
public abstract class ResettableScriptableObject:ScriptableObject
{
    public abstract void Reset();
}

