using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroManager : MonoBehaviour
{
    public static HeroManager instance;
    public static int savedHeroIndex = 1;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [SerializeField] private HeroSO[] allHeroes;

    [SerializeField] private Entity playerHeroEntity;
    [SerializeField] private Entity otherHeroEntity;

    [SerializeField] private Button playerHeroAbilityButton;
    [SerializeField] private Image playerHeroImage;
    [SerializeField] private Image otherHeroImage;
    [SerializeField] private Image playerAbilityImage;
    [SerializeField] private Image otherAbilityImage;

    private Item currentPlayerHeroAbility;
    private bool isCanAbility;
    private bool isTargetingSpriteOn;

    private Item currentOtherHeroAbility;
    private bool isCanOtherAbility;


    private TurnManager turnManager;
    private EffectManager effectManager;

    void Start()
    {
        turnManager = TurnManager.Instance;
        effectManager = FindAnyObjectByType<EffectManager>();

        turnManager.UnsubscribeOnTurnStarted(OnTurnStarted);
        turnManager.SubscribeOnTurnStarted(OnTurnStarted);

        int myHeroIndex = savedHeroIndex;
        int randomOtherHeroIndex = Random.Range(0, allHeroes.Length);

        SetupGame(myHeroIndex, randomOtherHeroIndex);
    }
    private void OnDestroy()
    {
        if(turnManager != null)
            turnManager.UnsubscribeOnTurnStarted(OnTurnStarted);
    }
    private void Update()
    {
        if (turnManager.isLoading || !turnManager.isMyTurn) return;

        if(isTargetingSpriteOn)
        {
            if(Input.GetMouseButtonDown(1))
            {
                CancelTargeting();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                Entity target = FindTargetEntity();
                if (target != null)
                {
                    UseAbility(target);
                }
            }
        }
        
    }
    public void OnClickHeroAbilityButton()
    {
        if (isTargetingSpriteOn)
        {
            CancelTargeting();
        }
        else
        {
            TryUsePlayerHeroAbility();
        }
    }

    //게임내 영웅 세팅
    public void SetupGame(int playerIndex, int otherIndex)
    {
        HeroSO playerData = allHeroes[playerIndex];
        HeroSO otherData = allHeroes[otherIndex];

        SetupEntity(playerHeroEntity, playerData);
        currentPlayerHeroAbility = playerData.heroAbility;
        if (playerAbilityImage != null)
            playerAbilityImage.sprite = playerData.heroAbilitySprite;
        if (playerHeroImage != null)
            playerHeroImage.sprite = playerData.heroSprite;

        SetupEntity(otherHeroEntity, otherData);
        currentOtherHeroAbility = otherData.heroAbility;
        otherAbilityImage.sprite =otherData.heroAbilitySprite;
        otherHeroImage.sprite = otherData.heroSprite;
    }    

    //영웅의 능력, 이미지, 체력 세팅
    public void SetupEntity(Entity entity, HeroSO heroData)
    {
        if (entity == null || heroData == null) return;

        entity.ChangeHeroImage(heroData.heroSprite);
        entity.maxHealth = heroData.health;
        entity.health = heroData.health;
        entity.UpdateHealthUI();
    }    

    //영웅 능력 사용에 관한 턴관리
    public void OnTurnStarted(bool isMIne)
    {
        if(isMIne)
        {
            isCanAbility = true;
            isTargetingSpriteOn = false;

            playerAbilityImage.gameObject.SetActive(true);
            if (playerHeroAbilityButton != null) playerHeroAbilityButton.interactable = true;
        }
        else
        {
            isCanAbility = false;
            isTargetingSpriteOn = false;

            playerAbilityImage.gameObject.SetActive(false);
            if (playerHeroAbilityButton != null) playerHeroAbilityButton.interactable = false;

            isCanOtherAbility = true;
        }
    }

    //각 영웅의 능력사용 조건
    void TryUsePlayerHeroAbility()
    {
        if(!isCanAbility) return;

        if (turnManager.myMana < currentPlayerHeroAbility.cardCost) return;

        if(TargetUtil.RequiresExternalTarget(currentPlayerHeroAbility))
        {
            Debug.Log("타겟팅 모드 진입 성공!");
            isTargetingSpriteOn = true;

            TurnManager.Instance.SetEndTurnButton(false);
        }
        else
        {
            UseAbility(null);
        }
    }
    public void TryUseOtherHeroAbility()
    {
        if (!isCanOtherAbility || currentOtherHeroAbility == null) return;
        if (turnManager.otherMana < currentOtherHeroAbility.cardCost) return;

        Entity target = null;

        if(TargetUtil.RequiresExternalTarget(currentOtherHeroAbility))
        {

            target = OtherAbilityTarget();
        }

        bool isSuccess = effectManager.RunAbilities(currentOtherHeroAbility, otherHeroEntity, target);

        if(isSuccess)
        {
            CardShowManager.Instance.ShowCard(currentOtherHeroAbility, false);
            turnManager.UseMana(false, currentOtherHeroAbility.cardCost);
            isCanOtherAbility = false;
        }
    }
    private Entity OtherAbilityTarget()
    {
        List<Entity> canTargets= new List<Entity>();

        canTargets.Add(playerHeroEntity);

        Entity[] entities = FindObjectsOfType<Entity>();

        foreach(Entity ent in entities)
        {
            if (ent == playerHeroEntity || ent == otherHeroEntity) continue;

            if(ent.isMine)
            { 
                canTargets.Add(ent);
            }
        }

        if(canTargets.Count > 0)
        {
            int randomTarget = Random.Range(0, canTargets.Count);
            return canTargets[randomTarget];
        }

        return playerHeroEntity;
    }

    //영웅 능력 발동
    void UseAbility(Entity target)
    {
        bool isSuccess = effectManager.RunAbilities(currentPlayerHeroAbility, playerHeroEntity, target);

        if(isSuccess)
        {
            CardShowManager.Instance.ShowCard(currentPlayerHeroAbility, true);

            turnManager.UseMana(true, currentPlayerHeroAbility.cardCost);

            isCanAbility = false;
            isTargetingSpriteOn=false;

            TurnManager.Instance.SetEndTurnButton(true);

            if (playerAbilityImage != null) playerAbilityImage.gameObject.SetActive(false);
            if (playerHeroAbilityButton != null) playerHeroAbilityButton.interactable = false;
        }
    }

    //영웅 능력 발동 취소 타게팅만
    void CancelTargeting()
    {
        isTargetingSpriteOn = false;

        TurnManager.Instance.SetEndTurnButton(true);
    }

    //타게팅할 객체 찾기
    private Entity FindTargetEntity()
    {
        Vector2 mousePos = Utils.MousePos;        
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
        
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Entity>();
        }

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;
        
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            
            Entity uiEntity = result.gameObject.GetComponentInParent<Entity>();

            if (uiEntity != null)
            {
                return uiEntity;
            }
        }

        return null;
    }
}
