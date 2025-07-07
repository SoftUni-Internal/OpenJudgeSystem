
import { useMemo } from 'react';
import { MdOutlineRemoveCircle } from 'react-icons/md';
import { Checkbox,
    FormControl,
    FormControlLabel,
    FormGroup,
    FormLabel,
    IconButton,
    TextareaAutosize,
    TextField } from '@mui/material';
import StyledTooltip from 'src/components/administration/common/styled-tooltip/StyledTooltip';

import { MEMORY_LIMIT, NAME, SOLUTION_SKELETON, TIME_LIMIT } from '../../../../common/labels';
import { SOLUTION_SKELETON_PLACEHOLDER } from '../../../../common/messages';
import { IProblemSubmissionType } from '../../../../common/types';
import {
    NEW_ADMINISTRATION_PATH,
    SUBMISSION_TYPE_DOCUMENTS_VIEW_PATH,
} from '../../../../common/urls/administration-urls';
import useDisableMouseWheelOnNumberInputs from '../../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import ViewButton from '../../common/view/ViewButton';

import styles from './ProblemSubmissionTypes.module.scss';

interface IProblemSubmissionTypesProps{
    onPropChange: Function;
    onStrategyRemoved: Function;
    strategy: IProblemSubmissionType;
    isDefaultStrategySelected: boolean;
    defaultSubmissionTypeId: number;
}

const ProblemSubmissionTypes = (props: IProblemSubmissionTypesProps) => {
    const { onPropChange, onStrategyRemoved, strategy, isDefaultStrategySelected, defaultSubmissionTypeId } = props;

    const isSelectDefaultCheckboxDisabled = useMemo(
        () => isDefaultStrategySelected && !(strategy.id === defaultSubmissionTypeId),
        [ isDefaultStrategySelected, defaultSubmissionTypeId, strategy.id ],
    );

    useDisableMouseWheelOnNumberInputs();

    return (
        <FormGroup
          sx={{
              display: 'flex',
              flexDirection: 'row',
              alignItems: 'center',
              justifyContent: 'space-around',
              margin: '1rem',
              alignSelf: 'flex-start',
              minWidth: '95%',
              flexWrap: 'wrap',
          }}
          key={strategy.id}
        >
            <IconButton
              sx={{ width: '5%' }}
              onClick={() => onStrategyRemoved(strategy.id)}
            >
                <MdOutlineRemoveCircle color="red" />
            </IconButton>
            <FormControl className={styles.submissionTypeName}>
                <TextField
                  variant="standard"
                  label={NAME}
                  value={strategy.name}
                  InputLabelProps={{ shrink: true }}
                  type="text"
                  InputProps={{ readOnly: true }}
                />
            </FormControl>
            <FormControl className={styles.solutionSkeleton}>
                <FormLabel>{SOLUTION_SKELETON}</FormLabel>
                <TextareaAutosize
                  placeholder={SOLUTION_SKELETON_PLACEHOLDER}
                  minRows={5}
                  maxRows={5}
                  value={strategy.solutionSkeleton ?? ''}
                  name="skeleton"
                  onChange={(e) => onPropChange(e.target.value, strategy.id, 'solutionSkeleton')}
                />
            </FormControl>
            <FormGroup className={styles.limitsWrapper}>
                <TextField
                  variant="standard"
                  label={TIME_LIMIT}
                  value={strategy.timeLimit ?? ''}
                  InputLabelProps={{ shrink: true }}
                  name="timeLimit"
                  type="number"
                  className={styles.limitField}
                  onChange={(e) => onPropChange(e.target.value, strategy.id, 'timeLimit')}
                />
                <TextField
                  variant="standard"
                  label={MEMORY_LIMIT}
                  value={strategy.memoryLimit ?? ''}
                  InputLabelProps={{ shrink: true }}
                  type="number"
                  name="memoryLimit"
                  className={styles.limitField}
                  onChange={(e) => onPropChange(e.target.value, strategy.id, 'memoryLimit')}
                />
            </FormGroup>
            <div className={styles.optionsWrapper}>
                <ViewButton
                  path={`/${NEW_ADMINISTRATION_PATH}/${SUBMISSION_TYPE_DOCUMENTS_VIEW_PATH}?submissionTypeIds=${strategy.id}`}
                  text={`View all documentation for ${strategy.name}`}
                />
                <StyledTooltip
                  placement="left"
                  arrow
                  title="Set as default strategy"
                  disabled={isSelectDefaultCheckboxDisabled}
                >
                    <FormControlLabel
                      sx={{ marginX: '0px' }}
                      label=""
                      control={<Checkbox checked={strategy.id === defaultSubmissionTypeId} disabled={isSelectDefaultCheckboxDisabled} />}
                      name="defaultSubmissionTypeId"
                      onChange={(e, checked) => onPropChange(checked, strategy.id, 'defaultSubmissionTypeId')}
                    />
                </StyledTooltip>

            </div>
        </FormGroup>
    );
};

export default ProblemSubmissionTypes;
