import { FaExternalLinkAlt } from 'react-icons/fa';

import { ButtonSize, LinkButton, LinkButtonType } from './Button';

import styles from './ExternalLink.module.scss';

interface IExternalLinkProps {
    to: string;
    text: string;
}

const ExternalLink = ({ text, to }: IExternalLinkProps) => (
    <LinkButton
      to={to}
      isToExternal
      size={ButtonSize.small}
      type={LinkButtonType.plain}
      internalClassName={styles.externalLink}
    >
        {text}
        <FaExternalLinkAlt className={styles.externalIcon} />
    </LinkButton>
);

export default ExternalLink;
