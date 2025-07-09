import { MdError } from 'react-icons/md';

import concatClassNames from '../../../utils/class-names';

import IconSize from './common/icon-sizes';
import Icon, { IIconProps } from './Icon';

import styles from './ErrorIcon.module.scss';

type IErrorIconProps = IIconProps

const ErrorIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IErrorIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={MdError}
    />
;

export default ErrorIcon;
