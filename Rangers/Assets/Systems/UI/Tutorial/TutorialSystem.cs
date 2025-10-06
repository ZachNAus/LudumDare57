using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class TutorialSystem : MonoBehaviour
{
    [Header("Tutorial Pages")]
    [SerializeField] private List<GameObject> tutorialPages = new List<GameObject>();

    [Header("Navigation")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;

    [SerializeField] private Button closeButton;

    [Header("Animation Settings")]
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private Ease transitionEase = Ease.OutCubic;
    [SerializeField] private float slideDistance = 200f;

    private int currentPageIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        SetupNavigation();
        InitializePages();

        closeButton.onClick.AddListener(HideTutorial);
    }

    private void SetupNavigation()
    {
        if (previousButton != null)
            previousButton.onClick.AddListener(PreviousPage);

        if (nextButton != null)
            nextButton.onClick.AddListener(NextPage);

        UpdateNavigationButtons();
    }

    private void InitializePages()
    {
        // Hide all pages except the first one
        for (int i = 0; i < tutorialPages.Count; i++)
        {
            if (tutorialPages[i] != null)
            {
                tutorialPages[i].SetActive(i == 0);
            }
        }
    }

    public void ShowTutorial()
    {
        gameObject.SetActive(true);
        currentPageIndex = 0;
        InitializePages();
        UpdateNavigationButtons();
    }

    public void HideTutorial()
    {
        gameObject.SetActive(false);
    }

    public void NextPage()
    {
        if (isTransitioning || currentPageIndex >= tutorialPages.Count - 1)
            return;

        SwitchToPage(currentPageIndex + 1, true);
    }

    public void PreviousPage()
    {
        if (isTransitioning || currentPageIndex <= 0)
            return;

        SwitchToPage(currentPageIndex - 1, false);
    }

    private void SwitchToPage(int targetIndex, bool isNext)
    {
        if (targetIndex < 0 || targetIndex >= tutorialPages.Count)
            return;

        isTransitioning = true;

        GameObject currentPage = tutorialPages[currentPageIndex];
        GameObject nextPage = tutorialPages[targetIndex];

        // Determine slide direction
        float exitDirection = isNext ? -slideDistance : slideDistance;
        float enterDirection = isNext ? slideDistance : -slideDistance;

        // Ensure next page is active and positioned off-screen
        RectTransform nextRect = nextPage.GetComponent<RectTransform>();
        nextPage.SetActive(true);
        nextRect.anchoredPosition = new Vector2(enterDirection, 0);

        // Animate current page out
        RectTransform currentRect = currentPage.GetComponent<RectTransform>();
        currentRect.DOAnchorPosX(exitDirection, transitionDuration)
            .SetEase(transitionEase)
            .OnComplete(() =>
            {
                currentPage.SetActive(false);
                currentRect.anchoredPosition = Vector2.zero;
            });

        // Animate next page in
        nextRect.DOAnchorPosX(0, transitionDuration)
            .SetEase(transitionEase)
            .OnComplete(() =>
            {
                isTransitioning = false;
                currentPageIndex = targetIndex;
                UpdateNavigationButtons();
            });
    }

    private void UpdateNavigationButtons()
    {
        if (previousButton != null)
            previousButton.interactable = currentPageIndex > 0;

        if (nextButton != null)
            nextButton.interactable = currentPageIndex < tutorialPages.Count - 1;
    }

    public void SetPage(int index)
    {
        if (index < 0 || index >= tutorialPages.Count || isTransitioning)
            return;

        bool isNext = index > currentPageIndex;
        SwitchToPage(index, isNext);
    }
}
