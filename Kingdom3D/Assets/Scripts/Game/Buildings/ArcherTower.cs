using Den.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArcherTower : MonoBehaviour, IBuilding
{
    public enum Levels
    {
        level1,
        level2,
        level3,
        level4
    }

    public Levels level;
    [SerializeField] Transform[] archerPositions;
    Archer[] archers;
    private float checkTime;

    protected void Start()
    {
        archers = new Archer[archerPositions.Length];
        checkTime = 1f;
    }

    protected void Update()
    {
        if (checkTime <= 0)
        {
            for (int i = 0; i < archers.Length; i++)
            {
                if (archers[i] == null)
                {
                    if (GetNearestAvailableArcher(transform.position, ref archers[i]))
                        archers[i].target = transform;
                    else
                        break;
                }
            }

            checkTime = 3f;
        }
        else
            checkTime -= Time.deltaTime;
    }

    public Transform GainArcher(Archer archer)
    {
        // called by an archer to teleport to their tower position

        return archerPositions[archers.Find(archer)];
    }

    public void Upgrade()
    {
        // TODO: free up archer


        // instantiate the next tower
        var obj = level == Levels.level1 ? ObjectReferences.Instance.tower2 :
                  level == Levels.level2 ? ObjectReferences.Instance.tower3 :
                  ObjectReferences.Instance.tower4;

        Instantiate(obj,
            transform.position,
            transform.rotation,
            transform.parent);
        Destroy(this);
    }

    static bool GetNearestAvailableArcher(Vector3 position, ref Archer archer)
    {
        archer = LevelController.Instance.subjects
            .GetComponentsInChildren<SubjectBase>() // get every subjectBase
            .OfType<Archer>() // remove any that aren't builders
            .Where(b => b.target == null) // remove any that are already at moving to a target
            .OrderBy(b => (b.transform.position - position).sqrMagnitude) // order by distance
            .FirstOrDefault();

        return archer != null;
    }
}
