using System.Text.Json.Nodes;

namespace TickerManageProgram
{
    internal class FormWatcher
    {
        const int detectDepth = 10;

        FormState formState;
        string formType;
        public FormWatcher(string ticker, string formType, string directory)
        {
            this.formType = formType;

            formState = new FormState(directory, "form" + formType);
        }

        public async Task<int[]> DetectAndApplyNewForm(JsonNode filings)
        {
            var filingDatesArr = filings["filingDate"].AsArray();
            var formsArr = filings["form"].AsArray();
            var accessionNumbersArr = filings["accessionNumber"].AsArray();
            int count = filingDatesArr.Count;

            int newFormsCount = 0;
            Stack<int> indexStack = new();
            for (int i = 0; i < count; i++)
            {
                if (newFormsCount >= detectDepth) // 탐색 깊이 제한
                { break; }

                DateTime filingDate = DateTime.Parse(filingDatesArr[i].GetValue<string>());

                if (filingDate < formState.latestDate) // 파일 날짜가 저장된 최근 날짜보다 전날이면 종료
                { break; }

                if (!string.Equals(formsArr[i].GetValue<string>(), formType)) // type 골라내기
                { continue; }

                string accessionNumber = accessionNumbersArr[i].GetValue<string>();

                if (filingDate > formState.latestDate)
                {
                    indexStack.Push(i);
                    newFormsCount++;
                    continue;
                }
                else // 파일 날짜가 저장된 최근 날짜와 같으면 기존의 accessionNumber와 대조
                {
                    if (formState.accessionNumbers.Contains(accessionNumber)) // 같은 날짜에 이미 등록된 파일이면 패스
                    { continue; }
                    indexStack.Push(i); // 없는 파일이면 등록
                    newFormsCount++;
                }
            }

            // 새로운 form들 스택 반영
            int[] newFormIndexArr = new int[newFormsCount];
            for (int i = 0; i < newFormsCount; i++)
            {
                int index = indexStack.Pop();
                DateTime newDateTime = DateTime.Parse(filingDatesArr[index].GetValue<string>());
                string newAccessionNumber = accessionNumbersArr[index].GetValue<string>();
                formState.UpdateLatestForm(newDateTime, newAccessionNumber);
                newFormIndexArr[i] = index;
            }
            return newFormIndexArr;
        }
    }
}
