import { Link } from 'react-router-dom';

import { IIndexContestsType } from '../../../common/types';
import useTheme from '../../../hooks/use-theme';
import { setProfile } from '../../../redux/features/usersSlice';
import { useAppDispatch } from '../../../redux/store';

import styles from './ProfileSearchList.module.scss';

interface IProfileSearchListProps {
    data: IIndexContestsType[] | undefined;
}
const ProfileSearchList = (props: IProfileSearchListProps) => {
    const dispatch = useAppDispatch();
    const { data } = props;
    const { getColorClassName, themeColors } = useTheme();

    const textColorClassName = getColorClassName(themeColors.textColor);

    if (!data) {
        return <div>No profiles data found</div>;
    }
    return (
        <div className={`${styles.profileElementsWrapper} ${textColorClassName}`}>
            {data.map((el) => 
                <Link
                  key={`p-l-i-${el.id}`}
                  to={`/profile/${el.name}`}
                  style={{ border: `1px solid ${themeColors.textColor}` }}
                  onClick={() => dispatch(setProfile(null))}
                >
                    {el.name}
                </Link>)}
        </div>
    );
};

export default ProfileSearchList;
