import { BiTime } from 'react-icons/bi';

import concatClassNames from '../../../utils/class-names';

import IconSize from './common/icon-sizes';
import Icon, { IIconProps } from './Icon';

import styles from './TimeLimitIcon.module.scss';

type IInProgressIconProps = IIconProps

const TimeLimitIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IInProgressIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      Component={BiTime}
      size={size}
      helperText={helperText}
    />
;

export default TimeLimitIcon;
