import { AiOutlineSolution } from 'react-icons/ai';
import { Link } from 'react-router-dom';
import { IconButton, Tooltip } from '@mui/material';

interface ISubmitSolutionProps {
    contestId: number;
    canBeCompeted: boolean;
    problemOrderBy?: number;
}

const SubmitSolution = (props:ISubmitSolutionProps) => {
    const { contestId, canBeCompeted, problemOrderBy = 1 } = props;
    return (
        <Tooltip title="Submit Solution">
            <IconButton>
                <Link
                  to={`/contests/${contestId}/${canBeCompeted
                      ? 'compete'
                      : 'practice'}#${problemOrderBy}`}
                  target="_blank"
                >
                    <AiOutlineSolution style={{ width: '40px', height: '40px', color: 'rgb(25,118,210)' }} color="primary" />
                </Link>
            </IconButton>
        </Tooltip>
    );
};
export default SubmitSolution;