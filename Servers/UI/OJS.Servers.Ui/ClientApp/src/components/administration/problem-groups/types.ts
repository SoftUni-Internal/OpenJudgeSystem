import { IContestAutocomplete } from '../../../common/types';

 
 interface IProblemGroupAdministrationModel{
    id: number;
    orderBy: number;
    type: string;
    contest: IContestAutocomplete;
}

export type {
    IProblemGroupAdministrationModel,
};
