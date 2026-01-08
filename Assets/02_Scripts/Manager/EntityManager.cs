using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] GameObject entityPrefab;
    [SerializeField] List<Entity> myEntities;
    [SerializeField] List<Entity> otherEntities;

    [SerializeField] Entity myEmptyEntity;
    [SerializeField] Entity myBossEntity;
    [SerializeField] Entity otherBossEntity;

    [Range(0f, 5f)][SerializeField] float entitySpacing = 2.3f;

    const int MAX_ENTITY_COUNT = 7;
    public bool isFullMyEntities => myEntities.Count >= MAX_ENTITY_COUNT && !ExistMyEmptyEntity;
    bool isFullOtherEntities => otherEntities.Count >= MAX_ENTITY_COUNT;
    bool ExistMyEmptyEntity => myEntities.Exists(x => x == myEmptyEntity);
    int myEmptyEntityIndex => myEntities.FindIndex(x => x == myEmptyEntity);

    void EntityAlignment(bool isMine)
    {
        float targetY = isMine ? -1.48f : 1.48f;
        var targetEntities = isMine ? myEntities : otherEntities;

        for(int i = 0; i < targetEntities.Count; i++)
        {
            float targetX = (targetEntities.Count - 1) * -(entitySpacing/ 2) + i * entitySpacing;

            var targetEntity = targetEntities[i];
            targetEntity.originPos = new Vector3(targetX, targetY, 0);
            targetEntity.MoveTransform(targetEntity.originPos, true, 0.5f);
            //targetEntity.GetComponent<Order>()?.SetOriginOrder(i);
        }
    }
    public void InsertMyEmptyEntity(float xPos)
    {
        if (isFullMyEntities) return;

        if(!ExistMyEmptyEntity)
            myEntities.Add(myEmptyEntity);

        Vector3 emptyEntitiesPos = myEmptyEntity.transform.position;
        emptyEntitiesPos.x = xPos;
        myEmptyEntity.transform.position = emptyEntitiesPos;

        int emptyEntityIndex = myEmptyEntityIndex;
        myEntities.Sort((entity1, entity2) => entity1.transform.position.x.CompareTo(entity2.transform.position.x));

        if (myEmptyEntityIndex != emptyEntityIndex)
            EntityAlignment(true);
    }
    public void RemoveMyEmptyEntity()
    {
        if (!ExistMyEmptyEntity) return;

        myEntities.RemoveAt(myEmptyEntityIndex);
        EntityAlignment(true);
    }
    public bool SpawnEntity(bool isMine, Item item, Vector3 spawnPos)
    {
        if(isMine)
        {
            if (isFullMyEntities || !ExistMyEmptyEntity) return false; 
        }
        else
        {
            if(isFullOtherEntities) return false; 
        }

        var entityObject = Instantiate(entityPrefab, spawnPos, Utils.QI);
        var entity = entityObject.GetComponent<Entity>();

        if (isMine)
            myEntities[myEmptyEntityIndex] = entity;
        else
            otherEntities.Insert(Random.Range(0, otherEntities.Count), entity);

        entity.isMine = isMine;
        entity.Setup(item);
        EntityAlignment(isMine);
        
        return true;
    }
}
