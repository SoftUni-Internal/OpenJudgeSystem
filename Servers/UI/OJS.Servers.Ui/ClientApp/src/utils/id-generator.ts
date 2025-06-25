import { v4 as uuidv4 } from 'uuid';

const generateId = () => uuidv4().toString();

const getCompositeKey = (userId: string, problemId: number): string => `${userId}_${problemId}`;

export {
    getCompositeKey,
};

export default generateId;
