import { BiMemoryCard } from 'react-icons/bi';

import IconSize from './common/icon-sizes';
import Icon, { IIconProps } from './Icon';

type IMemoryIconProps = IIconProps

const MemoryIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IMemoryIconProps) => 
    <Icon
      className={className}
      Component={BiMemoryCard}
      size={size}
      helperText={helperText}
    />
;

export default MemoryIcon;
