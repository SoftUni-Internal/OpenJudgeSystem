import { BiMemoryCard } from 'react-icons/bi';

import concatClassNames from '../../../utils/class-names';

import IconSize from './common/icon-sizes';
import Icon, { IIconProps } from './Icon';

import styles from './MemoryLimitIcon.module.scss';

type IInProgressIconProps = IIconProps

const MemoryLimitIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IInProgressIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      Component={BiMemoryCard}
      size={size}
      helperText={helperText}
    />
;

export default MemoryLimitIcon;
