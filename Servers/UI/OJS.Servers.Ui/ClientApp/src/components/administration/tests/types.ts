interface ITestAdministration{
    id: number;
    input: string;
    output: string;
    type: string;
    orderBy: number;
    retestProblem: boolean;
    hideInput: boolean;
    problemId: number;
    problemName: string;
}

interface ITestInListData {
    id:number;
    isTrialTest:boolean;
    isOpenTest:boolean;
    hideInput:boolean;
    orderBy:number;
    problemId:number;
    problemName:string;
    retestProblem: boolean;
}

enum TestTypes {
    'Standard' = 0,
    'Trial' = 1,
    'Open' = 2,
}

export { TestTypes };
export type { ITestAdministration, ITestInListData };
