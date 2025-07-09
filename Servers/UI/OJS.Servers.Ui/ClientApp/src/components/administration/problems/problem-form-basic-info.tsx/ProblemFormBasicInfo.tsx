import {
    Autocomplete,
    Box,
    Checkbox,
    FormControl,
    FormControlLabel,
    MenuItem,
    TextField,
    Typography,
} from '@mui/material';
import isNaN from 'lodash/isNaN';

import { ContestVariation } from '../../../../common/contest-types';
import { ProblemGroupTypes } from '../../../../common/enums';
import { CHECKER, CONTEST_NAME, MAXIMUM_POINTS, MEMORY_LIMIT, NAME, ORDER_BY, PROBLEM_GROUP_TYPE, SHOW_DETAILED_FEEDBACK, SOURCE_CODE_SIZE_LIMIT, TIME_LIMIT } from '../../../../common/labels';
import { IProblemAdministration, IProblemGroupDropdownModel } from '../../../../common/types';
import useDisableMouseWheelOnNumberInputs from '../../../../hooks/common/use-disable-mouse-wheel-on-number-inputs';
import { useGetCheckersForProblemQuery } from '../../../../redux/services/admin/checkersAdminService';

import formStyles from '../../common/styles/FormStyles.module.scss';

interface IProblemFormBasicInfoProps {
    onChange: Function;
    currentProblem: IProblemAdministration;
    problemGroups : Array<IProblemGroupDropdownModel>;
}
const ProblemFormBasicInfo = (props: IProblemFormBasicInfoProps) => {
    const { onChange, currentProblem, problemGroups } = props;
    const { data: checkers } = useGetCheckersForProblemQuery(null);

    useDisableMouseWheelOnNumberInputs();

    const renderProblemGroups = () => {
        if (currentProblem.contestType === ContestVariation.OnlinePracticalExam) {
            const NEW_PROBLEM_GROUP: IProblemGroupDropdownModel = {
                id: 0,
                orderBy: 0,
            };
            const options = [ NEW_PROBLEM_GROUP, ...problemGroups ];

            return (
                <Box className={formStyles.row}>
                    <Box className={formStyles.row}>
                        <FormControl className={formStyles.inputRow}>
                            <Autocomplete<IProblemGroupDropdownModel>
                              sx={{ width: '100%' }}
                              className={formStyles.inputRow}
                              onChange={(event, newValue) => {
                                  onChange({ target: { name: 'problemGroupId', value: newValue?.id } });
                              }}
                              value={options.find((pg) => pg.id === currentProblem.problemGroupId) ?? null}
                              options={options}
                              getOptionLabel={(option) => {
                                  if (option.id === NEW_PROBLEM_GROUP.id) {
                                      return 'Add in a new problem group';
                                  }
                                  return option.orderBy.toString();
                              }}
                              renderInput={(params) =>
                                  <TextField {...params} label="Problem Group Order By" />
                              }
                              renderOption={(properties, option) => {
                                  if (option.id === NEW_PROBLEM_GROUP.id) {
                                      return (
                                          <MenuItem {...properties} key={option.id} value={option.id}>
                                              Add in a new problem group
                                          </MenuItem>
                                      );
                                  }
                                  return (
                                      <MenuItem {...properties} key={option.id} value={option.id}>
                                          {option.orderBy}
                                      </MenuItem>
                                  );
                              }}
                            />
                        </FormControl>
                    </Box>
                    <Box className={formStyles.row}>
                        {currentProblem.problemGroupId === NEW_PROBLEM_GROUP.id &&
                            <FormControl className={formStyles.inputRow}>
                                <Autocomplete
                                  sx={{ width: '100%' }}
                                  className={formStyles.inputRow}
                                  onChange={(event, newValue) => onChange({ target: { name: 'problemGroupType', value: newValue } })}
                                  value={currentProblem.problemGroupType}
                                  options={Object.keys(ProblemGroupTypes).filter((key) => isNaN(Number(key)))}
                                  renderInput={(params) =>
                                      <TextField {...params} label={PROBLEM_GROUP_TYPE} />
                                  }
                                  getOptionLabel={(option) => option}
                                  renderOption={(properties, option) =>
                                      <MenuItem {...properties} key={option} value={option}>
                                          {option}
                                      </MenuItem>
                                  }
                                />
                            </FormControl>
                        }
                    </Box>
                </Box>
            );
        }

        return (
            <Box className={formStyles.row}>
                <FormControl className={formStyles.inputRow}>
                    <Autocomplete
                      sx={{ width: '100%' }}
                      className={formStyles.inputRow}
                      onChange={(event, newValue) => onChange({ target: { name: 'problemGroupType', value: newValue } })}
                      value={currentProblem.problemGroupType}
                      options={Object.keys(ProblemGroupTypes).filter((key) => isNaN(Number(key)))}
                      renderInput={(params) =>
                          <TextField {...params} label={PROBLEM_GROUP_TYPE} />
                      }
                      getOptionLabel={(option) => option}
                      renderOption={(properties, option) =>
                          <MenuItem {...properties} key={option} value={option}>
                              {option}
                          </MenuItem>
                      }
                    />
                </FormControl>
            </Box>
        );
    };

    return (
        <>
            <Box className={formStyles.fieldBox}>
                <Typography className={formStyles.fieldBoxTitle} variant="h5">
                    General Information
                </Typography>
                <div className={formStyles.fieldBoxDivider} />
                <Box className={formStyles.fieldBoxElement}>
                    <Box className={formStyles.row}>
                        <TextField
                          className={formStyles.inputRow}
                          variant="standard"
                          label={NAME}
                          value={currentProblem?.name}
                          InputLabelProps={{ shrink: true }}
                          type="text"
                          name="name"
                          onChange={(e) => onChange(e)}
                        />
                        <TextField
                          className={formStyles.inputRow}
                          variant="standard"
                          label={CONTEST_NAME}
                          value={currentProblem?.contestName}
                          InputLabelProps={{ shrink: true }}
                          type="text"
                          name="contestName"
                          onChange={(e) => onChange(e)}
                          disabled
                        />
                    </Box>
                    <Box className={formStyles.row}>
                        <TextField
                          className={formStyles.inputRow}
                          variant="standard"
                          label={MAXIMUM_POINTS}
                          value={currentProblem?.maximumPoints}
                          InputLabelProps={{ shrink: true }}
                          type="number"
                          name="maximumPoints"
                          onChange={(e) => onChange(e)}
                        />
                        <TextField
                          className={formStyles.inputRow}
                          variant="standard"
                          label={SOURCE_CODE_SIZE_LIMIT}
                          value={currentProblem?.sourceCodeSizeLimit}
                          InputLabelProps={{ shrink: true }}
                          type="number"
                          name="sourceCodeSizeLimit"
                          onChange={(e) => onChange(e)}
                        />
                    </Box>
                    <Box className={formStyles.row}>
                        <TextField
                          className={formStyles.inputRow}
                          variant="standard"
                          label={TIME_LIMIT}
                          value={currentProblem?.timeLimit}
                          InputLabelProps={{ shrink: true }}
                          type="number"
                          name="timeLimit"
                          onChange={(e) => onChange(e)}
                        />
                        <TextField
                          className={formStyles.inputRow}
                          variant="standard"
                          label={MEMORY_LIMIT}
                          value={currentProblem?.memoryLimit}
                          InputLabelProps={{ shrink: true }}
                          type="number"
                          name="memoryLimit"
                          onChange={(e) => onChange(e)}
                        />
                    </Box>
                </Box>
            </Box>
            <Box className={formStyles.fieldBox}>
                <Typography className={formStyles.fieldBoxTitle} variant="h5">
                    Settings
                </Typography>
                <div className={formStyles.fieldBoxDivider} />
                <Box className={formStyles.fieldBoxElement}>
                    <Box className={formStyles.row}>
                        <FormControl className={formStyles.inputRow}>
                            <Autocomplete
                              sx={{ width: '100%' }}
                              className={formStyles.inputRow}
                              onChange={(event, newValue) => onChange({ target: { name: 'checkerId', value: newValue?.id } })}
                              value={checkers?.find((c) => c.id === Number(currentProblem.checkerId)) ?? null}
                              options={checkers ?? []}
                              getOptionLabel={(option) => option.name}
                              renderInput={(params) =>
                                  <TextField {...params} label={CHECKER} />
                              }
                              renderOption={(properties, option) =>
                                  <MenuItem {...properties} key={option.id} value={option.id}>
                                      {option.name}
                                  </MenuItem>
                              }
                            />
                        </FormControl>
                    </Box>
                    {renderProblemGroups()}
                    <Box className={formStyles.row}>
                        <TextField
                          className={formStyles.fieldBoxElementLeft}
                          variant="standard"
                          label={ORDER_BY}
                          value={currentProblem?.orderBy}
                          InputLabelProps={{ shrink: true }}
                          type="number"
                          name="orderBy"
                          onChange={(e) => onChange(e)}
                        />
                    </Box>
                    <Box className={formStyles.fieldBoxCheckBoxes}>
                        <FormControlLabel
                          control={<Checkbox checked={currentProblem.showDetailedFeedback} />}
                          label={SHOW_DETAILED_FEEDBACK}
                          name="showDetailedFeedback"
                          onChange={(e) => onChange(e)}
                        />
                    </Box>
                </Box>
            </Box>
        </>
    );
};
export default ProblemFormBasicInfo;
