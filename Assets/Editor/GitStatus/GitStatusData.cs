using System;

namespace GitStatus
{
    public class GitStatusData
    {
        // Git 저장소 정보
        public bool IsGitRepository { get; set; } // 레포지트리 여부 
        public string CurrentBranch { get; set; } = "알 수 없음"; // 현재 브랜치 
        public int UncommittedChanges { get; set; } // 커밋되지 않은 변경사항 수
        public int UnpulledCommits { get; set; } // 풀되지 않은 커밋 수
        public string LastUpdateTime { get; set; } = "없음"; // 마지막 업데이트 시간

        // 오류 정보
        public string ErrorMessage { get; set; } = ""; // 오류 메시지

        // 이니셜라이징
        public void Reset()
        {
            IsGitRepository = false;
            CurrentBranch = "알 수 없음";
            UncommittedChanges = 0;
            UnpulledCommits = 0;
            ErrorMessage = "";
        }
    }
}