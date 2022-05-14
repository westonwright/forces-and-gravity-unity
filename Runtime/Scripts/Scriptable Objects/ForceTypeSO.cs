using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ForceTypeSO", menuName = "Forces/ForceTypeSO")]
public class ForceTypeSO : ScriptableObject
{
    [SerializeField]
    [Tooltip("What type of force mode producers should apply to rigidbodies.")]
    private ForceMode ForceMode = ForceMode.Force;
    [SerializeField]
    [Tooltip("The color that will be used to draw gizmos for producers using this Force Type.")]
    private Color PreviewColor = Color.white;
    [SerializeField]
    [Tooltip(
        "Additional force types which can overwrite this force if they have a higher priority and aren't additive." +
        "All other Forces will interact additively at all times." +
        "Forces of the same type always overwrite each other based on priority if not set to additive."
        )]
    private List<ForceTypeSO> MixesWith = new List<ForceTypeSO>();

    public ForceMode forceMode { get { return ForceMode; } }
    public Color previewColor { get { return PreviewColor; } }
    public List<ForceTypeSO> mixesWith { get { return MixesWith; } }


    private void Reset()
    {
        ForceMode = ForceMode.Force;
        MixesWith.Add(this);
        PreviewColor = Random.ColorHSV(0, 1, .5f, 1, 1, 1, 1, 1);
    }
    private void OnValidate()
    {
        Dictionary<ForceTypeSO, int> forceTypeCounts = new Dictionary<ForceTypeSO, int>();

        foreach(ForceTypeSO ft in MixesWith)
        {
            if (forceTypeCounts.ContainsKey(ft))
            {
                forceTypeCounts[ft] += 1;
            }
            else
            {
                forceTypeCounts.Add(ft, 1);
            }
        }

        foreach(KeyValuePair<ForceTypeSO, int> pair in forceTypeCounts)
        {
            if(pair.Key != null)
            {
                int count = pair.Value;
                while (count > 1)
                {
                    MixesWith.Remove(pair.Key);
                    count -= 1;
                }
            }
        }

        if (!MixesWith.Contains(this))
        {
            //Debug.LogWarning("You cannot remove a 'Force Type' from its own list of mixers");
            MixesWith.Insert(0, this);
            //Debug.LogWarning("You cannot remove a 'Force Type' from its own list of mixers");
        }
        else
        {
            int index = MixesWith.IndexOf(this);
            if(index != 0)
            {
                MixesWith.RemoveAt(index);
                MixesWith.Insert(0, this);
            }
        }
    }
}
