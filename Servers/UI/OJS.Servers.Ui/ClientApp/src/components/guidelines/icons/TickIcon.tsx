import { FaCheck } from 'react-icons/fa';

import concatClassNames from '../../../utils/class-names';

import IconSize from './common/icon-sizes';
import Icon, { IIconProps } from './Icon';

import styles from './TickIcon.module.scss';

type ITickIconProps = IIconProps

const TickIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: ITickIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={FaCheck}
    />
;

export default TickIcon;
