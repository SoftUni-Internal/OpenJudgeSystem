import React, { memo } from 'react';
import { AiOutlineUpload } from 'react-icons/ai';

import concatClassNames from '../../../../utils/class-names';
import IconSize from '../common/icon-sizes';
import Icon, { IIconProps } from '../Icon';

import styles from './StrategyIcon.module.scss';

type IFileUploadIconProps = IIconProps

const FileUploadIcon = ({
    className = '',
    size = IconSize.Medium,
    helperText = '',
}: IFileUploadIconProps) => 
    <Icon
      className={concatClassNames(styles.icon, className)}
      size={size}
      helperText={helperText}
      Component={AiOutlineUpload}
    />
;

export default memo(FileUploadIcon);
