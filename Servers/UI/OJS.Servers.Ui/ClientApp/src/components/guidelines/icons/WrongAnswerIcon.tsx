import { FaTimes } from 'react-icons/fa';

import concatClassNames from '../../../utils/class-names';

import IconSize from './common/icon-sizes';
import Icon, { IIconProps } from './Icon';

import styles from './WrongAnswerIcon.module.scss';

type IErrorIconProps = IIconProps

const WrongAnswerIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IErrorIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={FaTimes}
    />
;

export default WrongAnswerIcon;
