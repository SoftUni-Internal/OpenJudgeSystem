import { FormControl, FormControlLabel, FormLabel, Radio, RadioGroup } from '@mui/material';

import formStyles from '../../common/styles/FormStyles.module.scss';

interface IContestCompetePracticeButtonsProps{
    value: number;
    setStateFunc: Function;
}
const ContestCompetePracticeButtons = (props: IContestCompetePracticeButtonsProps) => {
    const { value, setStateFunc } = props;

    const handleCHange = (e:any) => {
        setStateFunc(e.target.value === 'Compete'
            ? 0
            : 1);
    };

    return (
        <FormControl className={formStyles.inputRow}>
            <FormLabel id="type">Choose type</FormLabel>
            <RadioGroup
              aria-labelledby="type"
              value={value === 0
                  ? 'Compete'
                  : 'Practice'}
              onChange={handleCHange}
            >
                <FormControlLabel value="Compete" control={<Radio />} label="Compete" />
                <FormControlLabel value="Practice" control={<Radio />} label="Practice" />
            </RadioGroup>
        </FormControl>
    );
};

export default ContestCompetePracticeButtons;
