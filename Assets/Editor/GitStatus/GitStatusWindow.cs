using UnityEditor;
using UnityEngine;

namespace GitStatus
{
    public class GitStatusWindow : EditorWindow
    {
        // 데이터 및 서비스 참조
        private GitStatusData _statusData;
        private GitService _gitService;

        // UI 상태
        private bool _showDetails = false;
        private bool _autoRefresh = true;
        private float _refreshInterval = 60f;
        private double _lastRefreshTime;

        [MenuItem("Window/Git 상태")]
        public static void ShowWindow()
        {
            var window = GetWindow<GitStatusWindow>("Git 상태");
            window.minSize = new Vector2(300, 200);
        }

        private void OnEnable()
        {
            // 서비스 및 데이터 초기화
            _gitService = new GitService();
            _statusData = new GitStatusData();

            // 초기 데이터 로드
            RefreshGitStatus();

            // 업데이트 콜백 등록
            EditorApplication.update += OnUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
        }

        private void OnUpdate()
        {
            if (_autoRefresh && EditorApplication.timeSinceStartup - _lastRefreshTime > _refreshInterval)
            {
                RefreshGitStatus();
                _lastRefreshTime = EditorApplication.timeSinceStartup;
                Repaint();
            }
        }

        private void RefreshGitStatus()
        {
            _gitService.UpdateStatus(_statusData);
        }

        private void OnGUI()
        {
            DrawHeader();

            if (!_statusData.IsGitRepository)
            {
                EditorGUILayout.HelpBox("현재 프로젝트는 Git 저장소가 아닙니다.", MessageType.Warning);
                return;
            }

            if (!string.IsNullOrEmpty(_statusData.ErrorMessage))
            {
                DrawErrorMessage();
                return;
            }

            DrawMainInfo();
            DrawStatusMessages();
            DrawDetailsSection();
            DrawSettings();
            DrawActionButtons();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Git 저장소 상태", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);
        }

        private void DrawErrorMessage()
        {
            EditorGUILayout.HelpBox(_statusData.ErrorMessage, MessageType.Error);
            if (GUILayout.Button("다시 시도"))
            {
                RefreshGitStatus();
            }
        }

        private void DrawMainInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 브랜치 정보
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("현재 브랜치:", GUILayout.Width(100));
            EditorGUILayout.LabelField(_statusData.CurrentBranch, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();

            // 변경사항 정보
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("변경사항:", GUILayout.Width(100));
            string changesText = _statusData.UncommittedChanges > 0 ?
                $"{_statusData.UncommittedChanges}개 파일 변경됨" : "없음";
            EditorGUILayout.LabelField(changesText);
            EditorGUILayout.EndHorizontal();

            // Pull 필요 정보
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Pull 필요:", GUILayout.Width(100));
            string pullText = _statusData.UnpulledCommits > 0 ?
                $"{_statusData.UnpulledCommits}개 커밋 필요" : "최신 상태";
            EditorGUILayout.LabelField(pullText);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawStatusMessages()
        {
            if (_statusData.UncommittedChanges > 0)
            {
                EditorGUILayout.HelpBox($"커밋되지 않은 변경사항이 {_statusData.UncommittedChanges}개 있습니다.",
                    MessageType.Info);
            }

            if (_statusData.UnpulledCommits > 0)
            {
                EditorGUILayout.HelpBox($"원격 저장소에 {_statusData.UnpulledCommits}개의 새 커밋이 있습니다.",
                    MessageType.Warning);
            }
        }

        private void DrawDetailsSection()
        {
            _showDetails = EditorGUILayout.Foldout(_showDetails, "상세 정보");

            if (_showDetails)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("마지막 업데이트:", _statusData.LastUpdateTime);
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSettings()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            bool newAutoRefresh = EditorGUILayout.ToggleLeft("자동 새로고침", _autoRefresh, GUILayout.Width(110));
            if (newAutoRefresh != _autoRefresh)
            {
                _autoRefresh = newAutoRefresh;
                _lastRefreshTime = EditorApplication.timeSinceStartup;
            }

            EditorGUILayout.LabelField("간격(초):", GUILayout.Width(60));
            _refreshInterval = EditorGUILayout.FloatField(_refreshInterval, GUILayout.Width(50));
            _refreshInterval = Mathf.Max(5, _refreshInterval);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawActionButtons()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("지금 새로고침"))
            {
                RefreshGitStatus();
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}