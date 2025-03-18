using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using System;

namespace GitStatus
{
    public class GitService
    {
        private readonly string _workingDirectory;

        public GitService()
        {
            _workingDirectory = Application.dataPath;
        }

        public void UpdateStatus(GitStatusData statusData)
        {
            try
            {
                // Git 저장소 확인
                string revParseResult = ExecuteGitCommand("rev-parse --is-inside-work-tree");
                statusData.IsGitRepository = revParseResult.Contains("true");

                if (!statusData.IsGitRepository)
                {
                    statusData.Reset();
                    return;
                }

                // 현재 브랜치 확인
                statusData.CurrentBranch = GetCurrentBranch();

                // 원격 저장소와 동기화
                ExecuteGitCommand("fetch");

                // Pull 필요한 커밋 수 확인
                statusData.UnpulledCommits = GetUnpulledCommitsCount(statusData.CurrentBranch);

                // 커밋되지 않은 변경사항 확인
                statusData.UncommittedChanges = GetUncommittedChangesCount();

                // 마지막 업데이트 시간 기록
                statusData.LastUpdateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                // 오류 메시지 초기화
                statusData.ErrorMessage = "";
            }
            catch (Exception e)
            {
                statusData.ErrorMessage = $"Git 정보 업데이트 중 오류 발생: {e.Message}";
                Debug.LogError(statusData.ErrorMessage);
            }
        }

        private string GetCurrentBranch()
        {
            string branch = ExecuteGitCommand("symbolic-ref --short HEAD");
            if (string.IsNullOrEmpty(branch))
            {
                // Detached HEAD 상태 처리
                string headCommit = ExecuteGitCommand("rev-parse --short HEAD");
                return $"Detached HEAD ({headCommit})";
            }
            return branch;
        }

        private int GetUnpulledCommitsCount(string branch)
        {
            string behindCount = ExecuteGitCommand($"rev-list --count HEAD..origin/{branch}");
            return int.TryParse(behindCount, out int count) ? count : 0;
        }

        private int GetUncommittedChangesCount()
        {
            string changes = ExecuteGitCommand("status --porcelain");
            return string.IsNullOrEmpty(changes) ? 0 :
                changes.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        private string ExecuteGitCommand(string command)
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = command,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _workingDirectory
                };

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    string error = process.StandardError.ReadToEnd().Trim();
                    process.WaitForExit();

                    if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
                    {
                        Debug.LogWarning($"Git 명령어 오류: {error}");

                        // Github Desktop에서 Branch 생성 및 변경을 시도하면, Origin Git에서 반영이 늦게되면서 경고 문구가 발생함.
                        // 이유 : GitHub Desktop에서 Branch를 생성하고, 어떠한 변경점도 Push 되지 않았다면, Origin Git에서는 해당 Branch가 존재하지 않는 것으로 판단함. 
                        // 해결 : 변경점을 Push 한 후에 Branch를 생성하면 해결됨.
                        // 참고 : https://github.com/desktop/desktop/issues/13440
                        
                    }

                    return output;
                }
            }
            catch (Exception e) 
            {
                Debug.LogError($"Git 명령어 실행 오류: {e.Message}");
                throw;
            }
        }
    }
}