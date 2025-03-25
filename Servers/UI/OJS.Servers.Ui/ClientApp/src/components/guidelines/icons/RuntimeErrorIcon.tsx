import { FaBell } from 'react-icons/fa';

import concatClassNames from '../../../utils/class-names';

import IconSize from './common/icon-sizes';
import Icon, { IIconProps } from './Icon';

import styles from './RuntimeErrorIcon.module.scss';

type IErrorIconProps = IIconProps

const RuntimeErrorIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IErrorIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={FaBell}
    />
;

export default RuntimeErrorIcon;
